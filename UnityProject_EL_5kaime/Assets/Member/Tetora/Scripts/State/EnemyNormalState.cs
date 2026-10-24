using UnityEngine;

public class EnemyNormalState : EnemyState
{

    public EnemyNormalState(Enemy enemy) : base(enemy)
    {
    }

    public override void Update()
    {
        // 67中のプレイヤーを検知した場合、発見ゲージがたまって追跡状態に遷移する
        if (Player != null)
        {
            if(Vector3.Distance(Enemy.transform.position, Player.transform.position) <= Enemy.detectionRange && Player.GetPlayerState() == PlayerController.PlayerState.SIXSEVEN)
            {
                Enemy.detectionGauge += Time.deltaTime;
                if (Enemy.detectionGauge >= 1f)
                {
                    Enemy.ChangeState(new EnemyChaseState(Enemy));
                }
            }
            else
            {
                Enemy.detectionGauge -= Time.deltaTime;
                if (Enemy.detectionGauge < 0f)
                {
                    Enemy.detectionGauge = 0f;
                }
            }
        }
    }
}
