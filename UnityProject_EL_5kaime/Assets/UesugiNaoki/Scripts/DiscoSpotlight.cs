using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Disco
{
    /// <summary>拍ごとにスポットライトを点滅させ、左右に振る。</summary>
    [RequireComponent(typeof(Light2D))]
    public class DiscoSpotlight : MonoBehaviour
    {
        [SerializeField] private DiscoBeat beat;
        [Tooltip("振る角度の幅(度)")]
        [SerializeField] private float swing = 25f;
        [Tooltip("振る速さ(rad/秒)")]
        [SerializeField] private float swingSpeed = 1.5f;
        [Tooltip("振りの位相ずらし(ライトごとに変える)")]
        [SerializeField] private float phase;
        [Tooltip("拍の後に暗くなっていく速さ")]
        [SerializeField] private float fadeSpeed = 3f;
        [SerializeField] private float minIntensity = 0.1f;
        [Tooltip("色相が一周する速さ(周/秒)")]
        [SerializeField] private float colorSpeed = 0.3f;

        private Light2D _light;
        private float _baseAngle;
        private float _level;

        private void Awake()
        {
            _light = GetComponent<Light2D>();
            _baseAngle = transform.eulerAngles.z;
            CreateMachine();
        }

        // ライトの向き(+Y)にレンズを向けた投光機。ライトと一緒に回る
        private void CreateMachine()
        {
            const int n = 32;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int py = 0; py < n; py++)
            for (int px = 0; px < n; px++)
            {
                var x = Mathf.Abs((px + 0.5f) / n * 2f - 1f);
                var y = (py + 0.5f) / n;
                Color c = Color.clear;
                if (y < 0.7f && x < Mathf.Lerp(0.3f, 0.5f, y / 0.7f)) c = new Color(0.35f, 0.35f, 0.35f, 1f);
                else if (y >= 0.7f && y < 0.85f && x < 0.55f) c = Color.white;
                tex.SetPixel(px, py, c);
            }
            tex.Apply();
            var go = new GameObject("Machine");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.78f), n);
            // ライトの影響を受けない素材にして、暗くても機械が見えるようにする
            sr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            sr.color = Color.black;
            sr.sortingOrder = 11;
            go.transform.localScale = Vector3.one * 1.2f;
        }

        private void OnEnable() => beat.OnBeat += HandleBeat;
        private void OnDisable() => beat.OnBeat -= HandleBeat;

        private void HandleBeat(int n) => _level = 1f;

        private void Update()
        {
            _level = Mathf.MoveTowards(_level, 0f, fadeSpeed * Time.deltaTime);
            _light.intensity = beat.IsPlaying ? Mathf.Lerp(minIntensity, 1.5f, _level) : 0f;
            _light.color = Color.HSVToRGB((Time.time * colorSpeed + phase * 0.15f) % 1f, 1f, 1f);
            transform.rotation =Quaternion.Euler(0, 0, _baseAngle + Mathf.Sin(Time.time * swingSpeed + phase) * swing);
        }
    }
}
