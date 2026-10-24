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
            // プレイヤーの位置に向かって移動する処理を追加
            Vector3 direction = (Player.transform.position - Enemy.transform.position).normalized;
            Enemy.transform.position += direction * Time.deltaTime * 2.0f; // 速度は適宜調整
        }
    }
}
