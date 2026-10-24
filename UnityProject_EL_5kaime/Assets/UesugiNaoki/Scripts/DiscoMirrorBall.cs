using UnityEngine;

namespace Disco
{
    /// <summary>コードで生成したミラーボールを吊るし、ゆっくり回して、拍でキラッと光らせる。</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DiscoMirrorBall : MonoBehaviour
    {
        private const int Frames = 16;
        private const int Size = 128;
        private const int Tiles = 8;

        [SerializeField] private DiscoBeat beat;
        [Tooltip("ボールの直径")]
        [SerializeField] private float diameter = 2.5f;
        [Tooltip("1 周で見える模様の流れる速さ(枚/秒)")]
        [SerializeField] private float spinSpeed = 32f;
        [Tooltip("拍の後に暗くなっていく速さ")]
        [SerializeField] private float fadeSpeed = 3f;
        [Tooltip("吊り線の上端の高さ")]
        [SerializeField] private float lineTop = 7f;

        private SpriteRenderer _sr;
        private Sprite[] _frames;
        private float _level;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _frames = new Sprite[Frames];
            for (int i = 0; i < Frames; i++) _frames[i] = MakeFrame(i / (float)Frames);
            transform.localScale = Vector3.one * diameter;
            CreateLine();
        }

        private void OnEnable() => beat.OnBeat += HandleBeat;
        private void OnDisable() => beat.OnBeat -= HandleBeat;

        private void HandleBeat(int n) => _level = 1f;

        private void Update()
        {
            _level = Mathf.MoveTowards(_level, 0f, fadeSpeed * Time.deltaTime);
            _sr.sprite = _frames[Mathf.FloorToInt(Time.time * spinSpeed) % Frames];
            var v = beat.IsPlaying ? Mathf.Lerp(0.9f, 1.8f, _level) : 0.2f;
            _sr.color = new Color(v, v, v, 1f);
        }

        // 球面の経度をずらしたタイル模様。1 タイル分ずらしきるとちょうど 1 周期になる
        private static Sprite MakeFrame(float shift)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var step = Mathf.PI / Tiles;
            for (int py = 0; py < Size; py++)
            for (int px = 0; px < Size; px++)
            {
                var x = (px + 0.5f) / Size * 2f - 1f;
                var y = (py + 0.5f) / Size * 2f - 1f;
                var z2 = 1f - x * x - y * y;
                if (z2 <= 0f) { tex.SetPixel(px, py, Color.clear); continue; }

                var u = (Mathf.Atan2(x, Mathf.Sqrt(z2)) / step) + shift;
                var w = Mathf.Asin(y) / step;
                var fu = u - Mathf.Floor(u);
                var fw = w - Mathf.Floor(w);
                var edge = fu < 0.08f || fu > 0.92f || fw < 0.08f || fw > 0.92f;
                var hash = Mathf.Abs(Mathf.Sin(Mathf.Floor(u) * 12.9898f + Mathf.Floor(w) * 78.233f) * 43758.5453f) % 1f;
                var c = edge ? 0.15f : Mathf.Lerp(0.8f, 1f, hash);
                c *= 0.7f + 0.3f * Mathf.Sqrt(z2);
                tex.SetPixel(px, py, new Color(c, c, c, 1f));
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        private void CreateLine()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            var go = new GameObject("Line");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0f), 1f);
            sr.color = new Color(0.5f, 0.5f, 0.5f);
            sr.sortingOrder = 9;
            // 親のスケール(直径)を打ち消してワールドで 0.04 幅にする
            var len = lineTop - transform.position.y - diameter * 0.5f;
            go.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            go.transform.localScale = new Vector3(0.04f / diameter, len / diameter, 1f);
        }
    }
}
