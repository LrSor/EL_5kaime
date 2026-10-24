using System.Collections;
using UnityEngine;

public class EffectSpriteObject : EffectObject
{

    [SerializeField]
    private float duration = 0.8f;

    [SerializeField]
    private float moveDistance = 0.5f;


    private Vector3 startPosition;

    public override void Play()
    {
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        startPosition = transform.position;

        float timer = 0f;


        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);

            // EaseOutQuint
            float easedT = 1f - Mathf.Pow(1f - t, 5f);

            transform.position =
                startPosition + Vector3.up * (moveDistance * easedT);

            yield return null;
        }
        Release();
    }
}
