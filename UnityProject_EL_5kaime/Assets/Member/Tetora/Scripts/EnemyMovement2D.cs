using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
public sealed class EnemyMovement2D : MonoBehaviour
{
    [Tooltip("Collider2Dから作ったXY平面のNavMeshで迂回する")]
    public bool useNavMesh = true;
    [Min(0.05f)] public float repathInterval = 0.35f;
    [Min(0.01f)] public float stoppingDistance = 0.05f;
    [Min(0.001f)] public float collisionSkin = 0.02f;

    private Enemy enemy;
    private CircleCollider2D feet;
    private Rigidbody2D body;
    private EnemyNavMesh2D navigation;
    private NavMeshPath path;
    private readonly List<RaycastHit2D> castHits = new List<RaycastHit2D>();
    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private Vector3[] corners;
    private int cornerIndex;
    private int navigationVersion = -1;
    private Vector2 pathDestination;
    private float repathTime;

    public Vector2 FeetPosition => transform.TransformPoint(feet.offset);
    public float Radius => feet.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
    public int ObstacleLayers => enemy.wanderSettings.obstacleLayers.value;

    private void Awake()
    {
        path = new NavMeshPath();
        enemy = GetComponent<Enemy>();
        feet = GetComponent<CircleCollider2D>();
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.useFullKinematicContacts = true;
    }

    private void OnEnable()
    {
        ResetPath();
    }

    public void ResetPath()
    {
        corners = null;
        cornerIndex = 0;
        repathTime = 0f;
    }

    // destination は画像の底中央（Enemy本体の原点）の目標位置。
    // NavMeshと接触判定には、その位置から足元Colliderの中心へずらして渡す。
    public Vector2 MoveTowards(Vector2 destination, float speed, float deltaTime)
    {
        if (!isActiveAndEnabled || speed <= 0f || deltaTime <= 0f) return Vector2.zero;
        Physics2D.SyncTransforms();
        ResolveOverlaps();
        Vector2 start = transform.position;
        Vector2 target = destination;
        if (useNavMesh)
        {
            if (navigation == null) navigation = EnemyNavMesh2D.GetOrCreate(this);
            navigation.EnsureReady(this);
            repathTime -= deltaTime;
            Vector2 footDestination = destination + FeetPosition - start;
            if (corners == null || repathTime <= 0f || navigationVersion != navigation.Version
                || (pathDestination - footDestination).sqrMagnitude > 0.0625f)
            {
                bool found = navigation.CalculatePath(FeetPosition, footDestination, path);
                corners = found ? path.corners : System.Array.Empty<Vector3>();
                cornerIndex = 0;
                pathDestination = footDestination;
                navigationVersion = navigation.Version;
                repathTime = repathInterval;
            }
            while (cornerIndex < corners.Length
                && Vector2.Distance(FeetPosition, corners[cornerIndex]) <= stoppingDistance)
            {
                cornerIndex++;
            }
            // 到達不能な場所へ直進したり、別の島へワープしたりしない。
            if (cornerIndex >= corners.Length) return Vector2.zero;
            target = (Vector2)corners[cornerIndex] - (FeetPosition - start);
        }

        Vector2 offset = target - start;
        if (offset.magnitude <= stoppingDistance) return Vector2.zero;
        Vector2 requested = Vector2.ClampMagnitude(offset, speed * deltaTime);
        MoveAndSlide(requested);
        Vector2 moved = (Vector2)transform.position - start;
        if (moved.sqrMagnitude < requested.sqrMagnitude * 0.01f) repathTime = 0f;
        return moved;
    }

    private ContactFilter2D ObstacleFilter()
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(ObstacleLayers);
        filter.useTriggers = false;
        return filter;
    }

    private bool Blocks(Collider2D other)
    {
        return other != null && !other.transform.IsChildOf(transform)
            && !Physics2D.GetIgnoreLayerCollision(gameObject.layer, other.gameObject.layer)
            && !Physics2D.GetIgnoreCollision(feet, other);
    }

    private void MoveAndSlide(Vector2 remaining)
    {
        // 足元から全移動距離をCastするので、高速時も薄い壁をすり抜けない。
        for (int iteration = 0; iteration < 3 && remaining.sqrMagnitude > 0.0000001f; iteration++)
        {
            float distance = remaining.magnitude;
            Vector2 direction = remaining / distance;
            Physics2D.CircleCast(FeetPosition, Radius, direction, ObstacleFilter(), castHits, distance + collisionSkin);
            float allowed = distance;
            Vector2 normal = Vector2.zero;
            foreach (var hit in castHits)
            {
                if (!Blocks(hit.collider) || Vector2.Dot(direction, hit.normal) >= -0.0001f) continue;
                float candidate = Mathf.Max(0f, hit.distance - collisionSkin);
                if (candidate <= allowed)
                {
                    allowed = candidate;
                    normal = hit.normal;
                }
            }
            Translate(direction * allowed);
            remaining -= direction * allowed;
            if (normal == Vector2.zero) break;
            remaining -= normal * Mathf.Min(0f, Vector2.Dot(remaining, normal));
        }
    }

    private void ResolveOverlaps()
    {
        // 配置時や、動く障害物が触れた場合の軽い重なりから抜け出す。
        for (int iteration = 0; iteration < 3; iteration++)
        {
            Physics2D.OverlapCircle(FeetPosition, Radius, ObstacleFilter(), overlaps);
            bool corrected = false;
            foreach (var other in overlaps)
            {
                if (!Blocks(other)) continue;
                ColliderDistance2D separation = feet.Distance(other);
                if (!separation.isValid || !separation.isOverlapped || separation.distance >= -0.0001f) continue;
                Translate(separation.normal * (separation.distance - collisionSkin));
                corrected = true;
            }
            if (!corrected) break;
        }
    }

    private void Translate(Vector2 displacement)
    {
        Vector3 position = transform.position + (Vector3)displacement;
        body.position = position;
        transform.position = position;
        Physics2D.SyncTransforms();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (corners == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 1; i < corners.Length; i++) Gizmos.DrawLine(corners[i - 1], corners[i]);
    }
#endif
}
