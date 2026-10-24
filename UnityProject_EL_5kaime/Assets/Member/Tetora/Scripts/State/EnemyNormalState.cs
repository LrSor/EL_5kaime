
using UnityEngine;

public class EnemyNormalState : EnemyState
{
    private float undetectedTime;
    private readonly EnemyWander wander;

    private float scoreAdd = 0f;
    private float scoreAddTotal = 0f;

    private const float ScoreInterval = 0.1f;
    private const float ScoreExponent = 4f;

    public EnemyNormalState(Enemy enemy) : base(enemy)
    {
        wander = new EnemyWander(enemy);
    }

    public override void Enter()
    {
        wander.Begin();

        undetectedTime = 0f;
        scoreAdd = 0f;
        scoreAddTotal = 0f;
    }

    public override void Update()
    {
        if (Player != null)
        {
            bool inSight = Enemy.IsPlayerInSight(Player);

            bool inRange =
                Vector3.Distance(
                    Enemy.transform.position,
                    Player.FeetPosition
                ) <= Enemy.detectionRange;

            bool isSixSeven =
                Player.GetPlayerState() ==
                PlayerController.PlayerState.SIXSEVEN;

            bool detected = inSight || (isSixSeven && inRange);

            // 発見ゲージ更新
            Enemy.detectionGauge =
                Enemy.normalGaugeSettings.UpdateGauge(
                    Enemy.detectionGauge,
                    detected,
                    Time.deltaTime,
                    ref undetectedTime
                );

            // SIXSEVEN中の近距離スコア加算
            if (isSixSeven && inRange)
            {
                UpdateScore(Time.deltaTime);
            }
            else
            {
                ResetScore();
            }

            // 視界に入った場合は即発見
            if (inSight)
            {
                Enemy.detectionGauge = 1f;
            }

            if (Enemy.detectionGauge >= 1f)
            {
                Enemy.ChangeState(new EnemyChaseState(Enemy));
                return;
            }
        }
        else
        {
            ResetScore();
        }

        wander.Update(Time.deltaTime);
    }

    private void UpdateScore(float deltaTime)
    {
        scoreAdd += deltaTime;
        scoreAddTotal += deltaTime;

        float maxTime = Mathf.Max(0.1f, Enemy.scoreMaxTime);
        int maxScore = Mathf.Max(1, Enemy.scoreMaxPerTick);

        while (scoreAdd >= ScoreInterval)
        {
            scoreAdd -= ScoreInterval;

            // スコア加算時点の経過時間
            float elapsed = scoreAddTotal - scoreAdd;

            // 0～1に正規化
            float t = Mathf.Clamp01(elapsed / maxTime);

            // 指数関数的な増加
            float growth =
                (Mathf.Exp(ScoreExponent * t) - 1f) /
                (Mathf.Exp(ScoreExponent) - 1f);

            // 1点から最大スコアまで増加
            int score = Mathf.RoundToInt(
                Mathf.Lerp(1f, maxScore, growth)
            );

            ScoreSystem.Instance?.ScoreAdd(score);
        }
    }

    private void ResetScore()
    {
        scoreAdd = 0f;
        scoreAddTotal = 0f;
    }
}
