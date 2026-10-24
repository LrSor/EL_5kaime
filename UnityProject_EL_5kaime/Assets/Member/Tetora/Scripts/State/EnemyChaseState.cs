using UnityEngine;

public class EnemyChaseState : EnemyState
{

    public EnemyChaseState(Enemy enemy) : base(enemy)
    {
    }

    public override void Update()
    {
        // Enemyを使って追跡処理
        if (Player != null)
        {
            if (Vector3.Distance(Enemy.transform.position, Player.transform.position) <= Enemy.detectionRange && Player.GetPlayerState() == PlayerController.PlayerState.SIXSEVEN)
            {
                Enemy.detectionGauge += Time.deltaTime;
                if (Enemy.detectionGauge >= 1f)
                {
                    Enemy.detectionGauge = 1f;
                }
                // プレイヤーの位置に向かって移動する処理を追加
                Vector3 direction = (Player.transform.position - Enemy.transform.position).normalized;
                Enemy.transform.position += direction * Time.deltaTime * 2.0f; // 速度は適宜調整
            }
            else
            {
                Enemy.detectionGauge -= Time.deltaTime;
                if (Enemy.detectionGauge < 0f)
                {
                    Enemy.detectionGauge = 0f;
                    Enemy.ChangeState(new EnemyNormalState(Enemy)); // プレイヤーが範囲外または状態が変わった場合、Idle状態に遷移
                }
            }
        }
    }
}
