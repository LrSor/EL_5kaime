using UnityEngine;

public class EnemyChaseState : EnemyState
{
    private float undetectedTime;

    public EnemyChaseState(Enemy enemy) : base(enemy)
    {
    }

    public override void Enter()
    {
        Enemy.ShowDetectionEffect(true);
    }

    public override void Update()
    {
        PlayerController player = Player;
        if (player != null)
        {
            Enemy.FaceTowards(player.FeetPosition);
        }

        // 追跡中はプレイヤーの状態や追跡距離ではなく、視界に入っているかで増減する。
        bool inSight = player != null && Enemy.IsInFieldOfView(player.SightTargetPosition);
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

        if (player != null && Vector3.Distance(Enemy.transform.position, player.FeetPosition) <= Enemy.chaseRange)
        {
            Enemy.Movement.MoveTowards(player.FeetPosition, Enemy.chaseSpeed, Time.deltaTime);

            if(Vector3.Distance(Enemy.transform.position, player.FeetPosition) <= Enemy.killRange)
            {
                Player.Stan();
                Enemy.ChangeState(new EnemyNormalState(Enemy));
                ScoreSystem.Instance.ScoreMinus(100);
                TimerSystem.Instance.TimeMinus(5f);
            }
        }
    }
}
