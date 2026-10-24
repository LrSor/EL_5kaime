using System.Collections;
using UnityEngine;

namespace SceneFlow
{
    /// <summary>
    /// 左右の手を、交互に上下させる(表示されている間だけ動く)。
    /// 片方が上がる時はもう片方が下がる。timeScale = 0 でも動くよう実時間で進める。
    /// </summary>
    public class HandSwing : MonoBehaviour
    {
        private const float TwoPi = Mathf.PI * 2f;

        [SerializeField, Tooltip("左の手")] private RectTransform _leftHand;
        [SerializeField, Tooltip("右の手")] private RectTransform _rightHand;
        [SerializeField, Tooltip("上下の振れ幅(基準位置から上下それぞれ)")] private float _amplitude = 150f;
        [SerializeField, Min(0f), Tooltip("1秒あたりの往復回数")] private float _cyclesPerSecond = 2f;

        private Coroutine _routine;
        private float _leftBaseY;
        private float _rightBaseY;

        private void Awake()
        {
            // 基準位置は、配置した位置をそのまま使う
            if (_leftHand != null) _leftBaseY = _leftHand.anchoredPosition.y;
            if (_rightHand != null) _rightBaseY = _rightHand.anchoredPosition.y;
        }

        private void OnEnable()
        {
            _routine = StartCoroutine(SwingRoutine());
        }

        private void OnDisable()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
            SetOffset(0f); // 次に表示する時は、必ず基準位置から始める
        }

        private IEnumerator SwingRoutine()
        {
            float startTime = Time.unscaledTime;
            while (true)
            {
                float phase = (Time.unscaledTime - startTime) * _cyclesPerSecond * TwoPi;
                SetOffset(Mathf.Sin(phase) * _amplitude);
                yield return null;
            }
        }

        // 左は +offset、右は -offset(交互に動く)
        private void SetOffset(float offset)
        {
            if (_leftHand != null) _leftHand.anchoredPosition = new Vector2(_leftHand.anchoredPosition.x, _leftBaseY + offset);
            if (_rightHand != null) _rightHand.anchoredPosition = new Vector2(_rightHand.anchoredPosition.x, _rightBaseY - offset);
        }
    }
}
