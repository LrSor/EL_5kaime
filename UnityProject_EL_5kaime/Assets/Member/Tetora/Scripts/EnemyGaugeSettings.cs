using UnityEngine;

[System.Serializable]
public sealed class EnemyGaugeSettings
{
    [Min(0f), Tooltip("ゲージが0からMAXになるまでの秒数。0で即MAX。視界での発見はこの値に関係なく即MAX")]
    public float fillTime = 1f;

    [Min(0f), Tooltip("ゲージがMAXから0になるまでの秒数。0で即0。減少開始までの猶予は別設定")]
    public float decreaseTime = 1f;

    [Min(0f), Tooltip("検知できなくなってからゲージが減り始めるまでの秒数。再検知すると猶予をリセット")]
    public float decreaseDelay = 0f;

    // 時間はゲージ全体を動かす時間。半分のゲージなら、所要時間も半分になる。
    public float UpdateGauge(float currentGauge, bool detected, float deltaTime, ref float undetectedTime)
    {
        currentGauge = Mathf.Clamp01(currentGauge);
        deltaTime = Mathf.Max(0f, deltaTime);
        if (detected)
        {
            undetectedTime = 0f;
            return fillTime <= 0f ? 1f : Mathf.MoveTowards(currentGauge, 1f, deltaTime / fillTime);
        }

        float remainingDelay = Mathf.Max(0f, decreaseDelay - undetectedTime);
        undetectedTime += deltaTime;
        if (deltaTime < remainingDelay)
        {
            return currentGauge;
        }

        // 猶予がフレーム途中で終わる場合、その後の時間分だけ減らす。
        float decreaseDeltaTime = deltaTime - remainingDelay;
        return decreaseTime <= 0f ? 0f
            : Mathf.MoveTowards(currentGauge, 0f, decreaseDeltaTime / decreaseTime);
    }
}
