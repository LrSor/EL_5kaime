using UnityEngine;

namespace Disco
{
    /// <summary>拍ごとに画面全体へ色を重ねる(部屋の手前に出る)。</summary>
    public class DiscoBackground : MonoBehaviour
    {
        [SerializeField] private DiscoBeat beat;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Color[] colors =
        {
            new Color(0.35f, 0f, 0.5f), new Color(0f, 0.2f, 0.6f),
            new Color(0.6f, 0f, 0.3f), new Color(0f, 0.45f, 0.4f),
        };
        [Tooltip("拍の後に暗くなっていく速さ(0 で減衰なし)")]
        [SerializeField] private float fadeSpeed = 2f;

        [Tooltip("重ねる色の濃さ(0〜1)")]
        [SerializeField, Range(0f, 1f)] private float alpha = 0.4f;
        [Tooltip("部屋より手前、演出より奥になる並び順")]
        [SerializeField] private int sortingOrder = 5;

        private Color _current;
        private float _level;
        private SpriteRenderer _overlay;

        private void Awake()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            var go = new GameObject("Overlay");
            go.transform.SetParent(transform, false);
            _overlay = go.AddComponent<SpriteRenderer>();
            _overlay.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            // ライトの影響を受けない素材にして、色をそのまま出す
            _overlay.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            _overlay.sortingOrder = sortingOrder;
        }

        private void OnEnable() => beat.OnBeat += HandleBeat;
        private void OnDisable() => beat.OnBeat -= HandleBeat;

        private void HandleBeat(int n)
        {
            _current = colors[n % colors.Length];
            _level = 1f;
        }

        private void Update()
        {
            if (fadeSpeed > 0f) _level = Mathf.MoveTowards(_level, 0.4f, fadeSpeed * Time.deltaTime);
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null) return;
            targetCamera.backgroundColor = Color.black;
            _overlay.enabled = beat.IsPlaying;
            var c = _current * _level;
            _overlay.color = new Color(c.r, c.g, c.b, alpha);
            // 画面全体を覆う(カメラに追従)
            var h = targetCamera.orthographicSize * 2f;
            _overlay.transform.position = new Vector3(targetCamera.transform.position.x, targetCamera.transform.position.y, 0f);
            _overlay.transform.localScale = new Vector3(h * targetCamera.aspect, h, 1f);
        }
    }
}
