using UnityEngine;

public class EnemyChaseState : EnemyState
{
    private float undetectedTime;

    public EnemyChaseState(Enemy enemy) : base(enemy)
    {
    }

    public override void Update()
    {
        PlayerController player = Player;
        if (player != null)
        {
            Enemy.FaceTowards(player.transform.position);
        }

        // 追跡中はプレイヤーの状態や追跡距離ではなく、視界に入っているかで増減する。
        bool inSight = player != null && Enemy.IsInFieldOfView(player.transform.position);
        Enemy.detectionGauge = Enemy.chaseGaugeSettings.UpdateGauge(
            Enemy.detectionGauge, inSight, Time.deltaTime, ref undetectedTime);
        if (inSight && player.GetPlayerState() == PlayerController.PlayerState.SIXSEVEN)
        {
            Enemy.detectionGauge = 1f;
        }

        if (!inSight && Enemy.detectionGauge <= 0f)
        {
            Enemy.ChangeState(new EnemyNormalState(Enemy));
            return;
        }

        if (player != null && Vector3.Distance(Enemy.transform.position, player.transform.position) <= Enemy.chaseRange)
        {
            Vector3 direction = (player.transform.position - Enemy.transform.position).normalized;
            Enemy.transform.position += direction * Time.deltaTime * Enemy.chaseSpeed;
        }
    }
}
