using UnityEngine;

public class EnemyChaseState : EnemyState
{
    private float undetectedTime;

    public EnemyChaseState(Enemy enemy) : base(enemy)
    {
    }

    public override void Update()
    {
        // Enemyを使って追跡処理
        if (Player != null)
        {
            Enemy.FaceTowards(Player.transform.position);
            bool withinChaseRange = Vector3.Distance(Enemy.transform.position, Player.transform.position) <= Enemy.chaseRange;
            bool inSight = Enemy.IsPlayerInSight(Player);
            bool detected = withinChaseRange || inSight;
            Enemy.detectionGauge = Enemy.chaseGaugeSettings.UpdateGauge(
                Enemy.detectionGauge, detected, Time.deltaTime, ref undetectedTime);
            if (inSight)
            {
                Enemy.detectionGauge = 1f;
            }

            if (withinChaseRange)
            {
                Vector3 direction = (Player.transform.position - Enemy.transform.position).normalized;
                Enemy.transform.position += direction * Time.deltaTime * Enemy.chaseSpeed;
            }
            else if (!detected && Enemy.detectionGauge <= 0f)
            {
                Enemy.ChangeState(new EnemyNormalState(Enemy));
            }
        }
    }
}
