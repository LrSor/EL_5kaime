using System;
using UnityEngine;
using UnityEngine.Events;

namespace Disco
{
    /// <summary>固定 BPM で拍を刻む。各演出はこの OnBeat を購読する。</summary>
    public class DiscoBeat : MonoBehaviour
    {
        [Tooltip("1 分あたりの拍数")]
        [SerializeField] private float bpm = 120f;
        [Tooltip("開始時に自動で再生するか")]
        [SerializeField] private bool playOnStart = true;
        [Tooltip("BGM。未設定なら鳴らさない(演出だけ動く)。ON のたびに頭から再生し、BPM は曲に合わせて設定する")]
        [SerializeField] private AudioClip bgm;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
        [SerializeField] private UnityEvent onEnabled;
        [SerializeField] private UnityEvent onDisabled;

        /// <summary>拍ごとに発火する。引数は拍の通し番号。</summary>
        public event Action<int> OnBeat;

        public bool IsPlaying { get; private set; }

        private float _timer;
        private int _count;
        private AudioSource _source;

        private void Start()
        {
            if (playOnStart) Enable();
        }

        /// <summary>演出を開始する。スコア等のイベントから呼ぶ。</summary>
        public void Enable()
        {
            if (IsPlaying) return;
            IsPlaying = true;
            _timer = 0f;
            _count = 0;
            if (bgm != null)
            {
                if (_source == null) _source = gameObject.AddComponent<AudioSource>();
                _source.clip = bgm;
                _source.volume = bgmVolume;
                _source.loop = true;
                _source.Play();
            }
            onEnabled?.Invoke();
            Fire();
        }

        /// <summary>演出を停止する。</summary>
        public void Disable()
        {
            if (!IsPlaying) return;
            IsPlaying = false;
            if (_source != null) _source.Stop();
            onDisabled?.Invoke();
        }

        /// <summary>外部から呼ぶ ON/OFF の入口。拍と、子の演出(ライト・紙吹雪・ミラーボール)の表示をまとめて切り替える。</summary>
        public void SetEffect(bool on)
        {
            foreach (Transform child in transform) child.gameObject.SetActive(on);
            if (on) Enable(); else Disable();
        }

        public void SetBpm(float value) => bpm = Mathf.Max(1f, value);

        private void Update()
        {
            if (!IsPlaying) return;
            _timer += Time.deltaTime;
            var interval = 60f / Mathf.Max(1f, bpm);
            while (_timer >= interval)
            {
                _timer -= interval;
                Fire();
            }
        }

        private void Fire() => OnBeat?.Invoke(_count++);
    }
}
