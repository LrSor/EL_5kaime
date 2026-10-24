using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SceneFlow
{
    /// <summary>
    /// ロード画面付きのシーンローダー。DontDestroyOnLoad で常駐するシングルトン。SceneLoader.Load("Game") で呼べる。
    /// Resources/PF_SceneLoader.prefab にアタッチする。prefab が無い場合は何もしない。
    /// </summary>
    public class SceneLoader : MonoBehaviour, ISceneLoader
    {
        private const string PrefabPath = "PF_SceneLoader"; // Resources/PF_SceneLoader.prefab
        private const float ProgressLoadedThreshold = 0.9f;
        private const float MinFillSeconds = 0.01f; // 0除算防止
        private const float PercentMax = 100f;
        private const string PercentFormat = "{0}%";

        private static SceneLoader _instance;

        [SerializeField, Tooltip("ロード画面全体の透明度を操作する部品。フェードに使い、表示中はクリックも遮断する。消さないこと")] private CanvasGroup _canvasGroup; // 全画面UI(Sort Order 大)
        [SerializeField, Tooltip("進捗バー(Slider)。0〜1の値を入れて表示する。消さないこと")] private Slider _progressBar;
        [SerializeField, Tooltip("進行%を表示するテキスト。空なら%を出さない")] private Text _percentLabel; // 進行％の表示(未設定なら出さない)
        [SerializeField, Tooltip("フェードインとフェードアウトそれぞれの秒数")] private float _fadeTime = 0.3f;
        [SerializeField, Tooltip("バーが0から1になるまでの最低秒数。読み込みが一瞬で終わってもこの時間をかけて進む")] private float _minDisplayTime = 1.0f;

        // 音などを鳴らす合図。音の基盤とは直接つながず、Inspector で鳴らす先を接続する
        [SerializeField, Tooltip("ロード開始時(フェードイン開始時)に呼ぶ処理")] private UnityEvent _onLoadStarted;      // ロード開始(フェードイン開始時)
        [SerializeField, Tooltip("バーが100%になった時に呼ぶ処理。途中で止めた場合は、そのぶん遅れる")] private UnityEvent _onProgressFilled;   // バーが満タンになった時
        [SerializeField, Tooltip("ロード画面が消え始める時に呼ぶ処理")] private UnityEvent _onFadeOutStarted;   // ロード画面が消え始める時
        [SerializeField, Tooltip("バーが指定%に達した時に呼ぶ処理と、その位置で止める時間。複数並べられる")] private List<ProgressTrigger> _progressTriggers = new List<ProgressTrigger>(); // バーが指定％に達した時

        private bool _isLoading;
        private int _shownPercent = -1; // 表示中の％(-1は未表示)

        private void Awake()
        {
            // _canvasGroup のオブジェクトは SetActive(false) しない
            // (同じオブジェクトにこのスクリプトがあるとコルーチンが止まる)
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        // 起動時に自動生成(どのシーンから Play しても存在する)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            // リポジトリ共有のため、prefab 未作成の環境(他メンバー)では黙ってスキップする
            var prefab = Resources.Load<SceneLoader>(PrefabPath);
            if (prefab == null) return;

            _instance = Instantiate(prefab);
            DontDestroyOnLoad(_instance.gameObject);
            SceneTransition.RegisterLoader(_instance); // 遷移の窓口(SceneTransition)から使えるようにする
        }

        private void OnDestroy()
        {
            SceneTransition.UnregisterLoader(this);
        }

        /// <summary>どこからでも呼べる。ロード中の多重呼び出しは無視する。</summary>
        public static void Load(string sceneName, Action onLoaded = null)
        {
            if (_instance == null || _instance._isLoading) return;
            _instance.StartCoroutine(_instance.LoadRoutine(sceneName, onLoaded));
        }

        // ISceneLoader: static の Load と同名のため明示実装にする
        bool ISceneLoader.IsLoading => _isLoading;

        void ISceneLoader.Load(string scenePath, Action onLoaded, Action onCompleted)
        {
            if (_isLoading) return;
            StartCoroutine(LoadRoutine(scenePath, onLoaded, onCompleted));
        }

        private IEnumerator LoadRoutine(string sceneName, Action onLoaded, Action onCompleted = null)
        {
            _isLoading = true;
            _progressBar.value = 0f; // 前回の満タンが、フェードイン中に見えないよう先に戻す
            foreach (var trigger in _progressTriggers) trigger.Reset();
            ShowPercent(0f);
            _onLoadStarted.Invoke();
            yield return Fade(0f, 1f);

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            // 実読み込みが一瞬で終わってもバーが見えるよう、表示値を一定速度で追従させる。
            // 0->1 に最低 _minDisplayTime 秒かかる(timeScale = 0 でも進むよう unscaledDeltaTime を使う)
            float shown = 0f;
            float fillSpeed = 1f / Mathf.Max(_minDisplayTime, MinFillSeconds);
            while (op.progress < ProgressLoadedThreshold || shown < 1f)
            {
                float target = Mathf.Clamp01(op.progress / ProgressLoadedThreshold);
                shown = Mathf.MoveTowards(shown, target, fillSpeed * Time.unscaledDeltaTime);
                _progressBar.value = shown;
                ShowPercent(shown);
                float hold = CheckProgressTriggers(shown);
                if (hold > 0f) yield return new WaitForSecondsRealtime(hold); // バーはその位置のまま止まる
                yield return null;
            }

            _progressBar.value = 1f;
            ShowPercent(1f);
            _onProgressFilled.Invoke();
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;

            yield return null; // 新シーンの初期化が一巡するのを1フレーム待つ
            onLoaded?.Invoke();

            _onFadeOutStarted.Invoke();
            yield return Fade(1f, 0f);
            _isLoading = false;
            onCompleted?.Invoke();
        }

        // バーと同じ値(0〜1)を整数%で表示する。毎フレーム文字列を作るのを避けるため、値が変わった時だけ更新する
        private void ShowPercent(float value)
        {
            if (_percentLabel == null) return;
            int percent = Mathf.FloorToInt(value * PercentMax);
            if (percent == _shownPercent) return;
            _shownPercent = percent;
            _percentLabel.text = string.Format(PercentFormat, percent);
        }

        // 計算量: 毎フレーム O(トリガー数)。トリガーは数個を想定
        // 同じフレームで複数発火した場合は、止める秒数を合計して返す
        private float CheckProgressTriggers(float shown)
        {
            float hold = 0f;
            foreach (var trigger in _progressTriggers) hold += trigger.Check(shown);
            return hold;
        }

        private IEnumerator Fade(float from, float to)
        {
            _canvasGroup.blocksRaycasts = true; // ロード中はクリックを遮断
            float t = 0f;
            while (t < _fadeTime)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, t / _fadeTime);
                yield return null;
            }
            _canvasGroup.alpha = to;
            _canvasGroup.blocksRaycasts = to > 0f; // 非表示時は遮断を解除
        }
    }
}
