using UnityEngine;

public class EnemyNormalState : EnemyState
{
    private float undetectedTime;

    public EnemyNormalState(Enemy enemy) : base(enemy)
    {
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
            if (inSight)
            {
                Enemy.detectionGauge = 1f;
            }

            if (Enemy.detectionGauge >= 1f)
            {
                Enemy.ChangeState(new EnemyChaseState(Enemy));
            }
        }
    }
}
