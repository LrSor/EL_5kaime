using System.Collections;
using UnityEngine;

public class EffectSpriteObject : EffectObject
{

    [SerializeField]
    private float duration = 0.8f;

    [SerializeField]
    private float moveDistance = 0.5f;


    private Vector3 startPosition;
    private Transform followTarget;
    private Vector3 followOffset;
    private bool holdUntilStopped;
    private float elapsed;

    public override void Play()
    {
        StopAllCoroutines();
        followTarget = null;
        holdUntilStopped = false;
        StartCoroutine(PlayAnimation());
    }

    // ステート表示用。親子付けせず、敵の回転・拡縮の影響を受けずに追従する。
    public void PlayFollowing(Transform target, Vector3 offset)
    {
        StopAllCoroutines();
        followTarget = target;
        followOffset = offset;
        holdUntilStopped = true;
        elapsed = 0f;
        transform.position = target.position + offset;
    }

    private void LateUpdate()
    {
        if (!holdUntilStopped)
        {
            return;
        }
        if (followTarget == null || !followTarget.gameObject.activeInHierarchy)
        {
            Stop();
            return;
        }

        elapsed += Time.deltaTime;
        float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
        float easedT = 1f - Mathf.Pow(1f - t, 5f);
        transform.position = followTarget.position + followOffset
            + Vector3.up * (moveDistance * easedT);
    }

    public void Stop()
    {
        if (gameObject.activeSelf)
        {
            Release();
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        followTarget = null;
        holdUntilStopped = false;
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
