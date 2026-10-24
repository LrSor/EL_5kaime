using System.Collections;
using UnityEngine;

public class EffectSpriteObject_ang : EffectObject
{

    [SerializeField]
    private float duration = 0.4f;

    [SerializeField]
    private float startScale = 0f;

    [SerializeField]
    private float endScale = 1f;

    public override void Play()
    {
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);

            float easedT = EaseOutBack(t);

            float scale = Mathf.LerpUnclamped(
                startScale,
                endScale,
                easedT
            );

            transform.localScale = Vector3.one * scale;

            yield return null;
        }

        // 最終値を確実に1にする
        transform.localScale = Vector3.one * endScale;

        Release();
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;

        return 1f
            + c3 * Mathf.Pow(t - 1f, 3f)
            + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
