using UnityEngine;

public class EnemyNormalState : EnemyState
{
    private float undetectedTime;
    private readonly EnemyWander wander;
    private float scoreAdd = 0f;
    public EnemyNormalState(Enemy enemy) : base(enemy)
    {
        wander = new EnemyWander(enemy);
    }

    public override void Enter()
    {
        wander.Begin();
        scoreAdd = 0f;
    }

    public override void Update()
    {
        // 67中の近距離検知では時間をかけて上昇。視界での発見は即MAX。
        if (Player != null)
        {
            bool inSight = Enemy.IsPlayerInSight(Player);
            bool detected = inSight || (Player.GetPlayerState() == PlayerController.PlayerState.SIXSEVEN
                && Vector3.Distance(Enemy.transform.position, Player.transform.position) <= Enemy.detectionRange);
            Enemy.detectionGauge = Enemy.normalGaugeSettings.UpdateGauge(
                Enemy.detectionGauge, detected, Time.deltaTime, ref undetectedTime);

            if (Player.GetPlayerState() == PlayerController.PlayerState.SIXSEVEN
                && Vector3.Distance(Enemy.transform.position, Player.transform.position) <= Enemy.detectionRange)
            {
                scoreAdd += Time.deltaTime;
                if (0.1f <= scoreAdd)
                {
                    ScoreSystem.Instance.ScoreAdd(Mathf.FloorToInt(scoreAdd * 10.0f));
                    scoreAdd -= 0.1f;
                }
            }
            else
            {
                scoreAdd = 0f;
            }

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

        wander.Update(Time.deltaTime);
    }
}
