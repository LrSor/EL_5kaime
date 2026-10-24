using UnityEngine;
using UnityEngine.Audio;

namespace Miyashita.Audio
{
    [DisallowMultipleComponent]
    public sealed class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [SerializeField] private SoundCatalog catalog;
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSource;
        [SerializeField] private AudioSource voiceSource;

        private float masterVolume = 1f;
        private float bgmVolume = 1f;
        private float seVolume = 1f;
        private float voiceVolume = 1f;
        private bool started;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetInstance() => Instance = null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Prefabを子に配置した場合も、このオブジェクトだけをシーン間で保持する。
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            bgmSource = EnsureSource(bgmSource, "BGM");
            seSource = EnsureSource(seSource, "SE");
            voiceSource = EnsureSource(voiceSource, "Voice");
        }

        private AudioSource EnsureSource(AudioSource source, string group)
        {
            if (source == null)
            {
                var child = new GameObject(group);
                child.transform.SetParent(transform, false);
                source = child.AddComponent<AudioSource>();
            }
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.loop = false;
            if (mixer != null)
            {
                var groups = mixer.FindMatchingGroups(group);
                if (groups.Length > 0) source.outputAudioMixerGroup = groups[0];
            }
            return source;
        }

        private void Start()
        {
            // AudioMixer.SetFloatはAwakeで呼ばず、Start以降に反映する。
            started = true;
            ApplyVolume("MasterVolume", masterVolume);
            ApplyVolume("BGMVolume", bgmVolume);
            ApplyVolume("SEVolume", seVolume);
            ApplyVolume("VoiceVolume", voiceVolume);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>カテゴリに応じて再生。未登録・素材なしなら何もせずfalse。</summary>
        public bool Play(string id)
        {
            if (catalog == null || !catalog.TryGet(id, out var entry) || entry.clip == null)
                return false;
            return PlayEntry(entry);
        }

        public bool PlayBGM(string id) => PlayCategory(id, SoundCategory.BGM);
        public bool PlaySE(string id) => PlayCategory(id, SoundCategory.SE);
        public bool PlayVoice(string id) => PlayCategory(id, SoundCategory.Voice);

        private bool PlayCategory(string id, SoundCategory category)
        {
            if (catalog == null || !catalog.TryGet(id, out var entry)
                || entry.clip == null || entry.category != category) return false;
            return PlayEntry(entry);
        }

        private bool PlayEntry(SoundCatalog.Entry entry)
        {
            if (Instance != this) return false;
            var volume = Mathf.Clamp01(entry.volume);
            switch (entry.category)
            {
                case SoundCategory.BGM:
                    PlaySingle(bgmSource, entry, volume);
                    return true;
                case SoundCategory.SE:
                    seSource.PlayOneShot(entry.clip, volume);
                    return true;
                case SoundCategory.Voice:
                    PlaySingle(voiceSource, entry, volume);
                    return true;
                default:
                    return false;
            }
        }

        private static void PlaySingle(AudioSource source, SoundCatalog.Entry entry, float volume)
        {
            source.Stop();
            source.clip = entry.clip;
            source.volume = volume;
            source.loop = entry.loop;
            source.Play();
        }

        public void StopBGM() { if (bgmSource != null) bgmSource.Stop(); }
        public void StopSE() { if (seSource != null) seSource.Stop(); }
        public void StopVoice() { if (voiceSource != null) voiceSource.Stop(); }
        public void StopAll() { StopBGM(); StopSE(); StopVoice(); }

        // Sliderの0～1を受け取る。設定の保存は呼び出し側で行う。
        public void SetMasterVolume(float value) => SetVolume("MasterVolume", value, ref masterVolume);
        public void SetBGMVolume(float value) => SetVolume("BGMVolume", value, ref bgmVolume);
        public void SetSEVolume(float value) => SetVolume("SEVolume", value, ref seVolume);
        public void SetVoiceVolume(float value) => SetVolume("VoiceVolume", value, ref voiceVolume);

        private void SetVolume(string parameter, float value, ref float storage)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return;
            storage = Mathf.Clamp01(value);
            if (started) ApplyVolume(parameter, storage);
        }

        private void ApplyVolume(string parameter, float value)
        {
            if (mixer == null) return;
            mixer.SetFloat(parameter, value <= 0f ? -80f : Mathf.Log10(value) * 20f);
        }
    }
}
