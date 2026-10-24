using UnityEngine;

[System.Serializable]
public sealed class EnemyWanderSettings
{
    public bool enabled = true;

    [Min(0f), Tooltip("通常時の歩行速度（ワールド単位 / 秒）。0で移動停止")]
    public float speed = 1f;

    [Min(0f), Tooltip("ゲーム開始位置を中心とする徘徊半径。0で徘徊停止。追跡後も中心は変えない")]
    public float radius = 5f;

    [Tooltip("一度に歩く時間の最小・最大（秒）")]
    public Vector2 walkTimeRange = new Vector2(1f, 3f);

    [Tooltip("立ち止まる時間の最小・最大（秒）")]
    public Vector2 waitTimeRange = new Vector2(0.5f, 1.5f);

    [Min(0f), Tooltip("歩く方向へ視界を向ける旋回速度（度 / 秒）。0で即座に向く")]
    public float turnSpeed = 120f;

    [Min(0f), Tooltip("旧プレファブ用の半径。足元にCircleCollider2Dがある敵は、その半径を使用")]
    public float collisionRadius = 0.5f;

    [Tooltip("徘徊・追跡とNavMesh生成で障害物とするCollider2Dのレイヤー。Triggerと自分は除外")]
    public LayerMask obstacleLayers = Physics2D.DefaultRaycastLayers;
}
