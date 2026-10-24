using System.Collections;
using UnityEngine;

namespace SceneFlow
{
    /// <summary>
    /// Show() を呼ぶと、指定したオブジェクトを一定時間だけ表示して、自動で非表示に戻す。
    /// ボタンの押下から呼んで、画像を一瞬だけ背景に出すのに使う。
    /// 表示中にもう一度呼ぶと、表示時間を数え直す。
    /// </summary>
    public class TimedImageToggle : MonoBehaviour
    {
        [SerializeField, Tooltip("一定時間だけ表示するオブジェクト(最初は非表示にしておく)")] private GameObject _target;
        [SerializeField, Min(0f), Tooltip("表示する秒数")] private float _seconds = 1f;

        private Coroutine _routine;

        /// <summary>表示を始める(ボタンの OnClick から呼ぶ)。</summary>
        public void Show()
        {
            if (_target == null) return;
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            _target.SetActive(true);
            yield return new WaitForSecondsRealtime(_seconds); // timeScale = 0 でも進むよう実時間で待つ
            _target.SetActive(false);
            _routine = null;
        }

        // このオブジェクトが無効になる・破棄される場合も、表示を残さない
        private void OnDisable()
        {
            _routine = null;
            if (_target != null) _target.SetActive(false);
        }
    }
}
