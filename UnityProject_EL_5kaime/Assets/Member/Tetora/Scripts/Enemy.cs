using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 100;
    public EnemyState currentState;

    public float detectionGauge = 0f;

    public float detectionRange = 5f;
    public float chaseRange = 10f;

    public float killRange = 0.5f;


    [Header("Score Settings")]
    [Min(0.1f)]
    public float scoreMaxTime = 2f;

    [Min(1)]
    public int scoreMaxPerTick = 20;


    [Header("通常時のランダムウォーク")]
    public EnemyWanderSettings wanderSettings = new EnemyWanderSettings();

    public Vector2 WanderOrigin { get; private set; }

    public EnemyMovement2D Movement { get; private set; }

    [Header("警戒ゲージ（秒）")]
    [Tooltip("通常時の近距離検知。視界での発見は常に即MAX")]
    public EnemyGaugeSettings normalGaugeSettings = new EnemyGaugeSettings();

    [Tooltip("追跡中は視界内で回復、視界外で猶予後に減少。67中の視界での発見は即MAX。Chase Rangeは移動範囲にのみ使用")]
    public EnemyGaugeSettings chaseGaugeSettings = new EnemyGaugeSettings();

    [Header("追跡の移動・向き")]
    [Min(0f), Tooltip("追跡速度（ワールド単位 / 秒）。0で移動停止")]
    public float chaseSpeed = 2f;

    [Min(0f), Tooltip("視界が旋回する速度（度 / 秒）。0で即座にプレイヤー方向を向く")]
    public float chaseTurnSpeed = 0f;

    [Header("通常時の視界 (2D)")]
    [Range(0f, 360f)]
    [Tooltip("視界の広さ（度）")]
    public float fieldOfViewAngle = 90f;

    [Range(0f, 360f)]
    [Tooltip("敵のローカル方向。0 = 右、90 = 上、180 = 左、270 = 下。通常時は歩行方向、追跡時はプレイヤーへ向く")]
    public float fieldOfViewDirection = 0f;

    [Min(0f)]
    [Tooltip("視界の長さ。近距離検知の Detection Range とは別に設定")]
    public float fieldOfViewLength = 5f;

    [SerializeField]
    private Color fieldOfViewColor = new Color(1f, 0.85f, 0f, 0.25f);

    [Header("追跡時の視界 (2D)")]
    [Range(0f, 360f)]
    public float chaseFieldOfViewAngle = 90f;

    [Min(0f)]
    public float chaseFieldOfViewLength = 5f;

    [SerializeField]
    private Color chaseFieldOfViewColor = new Color(1f, 0.85f, 0f, 0.25f);

    [Header("視界の共通設定")]
    [Tooltip("足元の原点から見た視界の起点（ローカル座標）。Yを顔の高さに合わせて調整")]
    public Vector2 visionOffset = new Vector2(0f, 1.5f);

    public Vector3 VisionOrigin => transform.TransformPoint(visionOffset);

    [Tooltip("視界を遮る Collider2D のレイヤー。Trigger・自分・プレイヤーは除外")]
    public LayerMask visionObstacleLayers = Physics2D.DefaultRaycastLayers;

    [SerializeField, Tooltip("背景より手前、キャラクターより奥になる値を設定")]
    private int fieldOfViewSortingOrder = -1;

    private const int VisionSegments = 90;
    private readonly List<RaycastHit2D> visionHits = new List<RaycastHit2D>();
    private readonly Vector3[] visionVertices = new Vector3[VisionSegments + 2];
    private readonly Color[] visionColors = new Color[VisionSegments + 2];
    private GameObject visionObject;
    private Mesh visionMesh;
    private MeshRenderer visionRenderer;

    private float CurrentVisionAngle => currentState is EnemyChaseState
        ? chaseFieldOfViewAngle : fieldOfViewAngle;
    private float CurrentVisionLength => currentState is EnemyChaseState
        ? chaseFieldOfViewLength : fieldOfViewLength;
    private Color CurrentVisionColor => currentState is EnemyChaseState
        ? chaseFieldOfViewColor : fieldOfViewColor;

    [SerializeField]
    private GameObject detectionMeter = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        Movement = GetComponent<EnemyMovement2D>();
        if (Movement == null)
        {
            if (GetComponent<CircleCollider2D>() == null)
            {
                var feet = gameObject.AddComponent<CircleCollider2D>();
                feet.radius = Mathf.Max(0.01f, wanderSettings.collisionRadius);
                feet.offset = Vector2.up * feet.radius;
            }
            Movement = gameObject.AddComponent<EnemyMovement2D>();
        }
        WanderOrigin = transform.position;
        detectionGauge = 0f;
        ChangeState(new EnemyNormalState(this));
        CreateVisionMesh();
    }

    // Update is called once per frame
    public void Update()
    {
        currentState?.Update();

        detectionGauge = Mathf.Clamp01(detectionGauge);
        if (detectionMeter != null)
        {
            detectionMeter.transform.localScale = new Vector3(detectionGauge, 0.1f, 1f);
        }
    }

    private void LateUpdate()
    {
        if (visionMesh == null)
        {
            return;
        }

        float angle = CurrentVisionAngle;
        float length = CurrentVisionLength;
        visionObject.transform.localPosition = visionOffset;
        visionRenderer.enabled = !(currentState is EnemyDeadState)
            && angle > 0f && length > 0f;
        visionRenderer.sortingOrder = fieldOfViewSortingOrder;
        visionVertices[0] = Vector3.zero;
        for (int i = 0; i <= VisionSegments; i++)
        {
            Vector2 direction = GetVisionDirection(-angle * 0.5f
                + angle * i / VisionSegments);
            float distance = GetVisibleDistance(direction, length);
            // ワールド単位の判定と、親の拡大縮小がある場合の表示を合わせる。
            visionVertices[i + 1] = transform.InverseTransformVector(direction * distance);
        }

        for (int i = 0; i < visionColors.Length; i++)
        {
            visionColors[i] = CurrentVisionColor;
        }

        visionMesh.vertices = visionVertices;
        visionMesh.colors = visionColors;
        visionMesh.RecalculateBounds();
    }

    private void OnEnable()
    {
        if (visionObject != null)
        {
            visionObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (visionObject != null)
        {
            visionObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (visionMesh != null)
        {
            Destroy(visionMesh);
        }
        if (visionObject != null)
        {
            Destroy(visionObject);
        }
    }

    public bool IsInFieldOfView(Vector3 targetPosition)
    {
        float length = CurrentVisionLength;
        float angle = CurrentVisionAngle;
        if (length <= 0f || angle <= 0f)
        {
            return false;
        }

        Vector2 offset = targetPosition - VisionOrigin;
        float distance = offset.magnitude;
        if (distance > length)
        {
            return false;
        }
        if (distance <= Mathf.Epsilon)
        {
            return true;
        }
        if (Vector2.Angle(GetVisionDirection(0f), offset) > angle * 0.5f)
        {
            return false;
        }

        return GetVisibleDistance(offset / distance, distance) >= distance;
    }

    public bool IsPlayerInSight(PlayerController player)
    {
        return player != null && player.GetPlayerState() == PlayerController.PlayerState.SIXSEVEN
            && IsInFieldOfView(player.SightTargetPosition);
    }

    public void FaceTowards(Vector3 targetPosition)
    {
        FaceTowards(targetPosition, chaseTurnSpeed, Time.deltaTime);
    }

    public void FaceTowards(Vector3 targetPosition, float turnSpeed, float deltaTime)
    {
        Vector2 direction = transform.InverseTransformDirection(targetPosition - transform.position);
        if (direction.sqrMagnitude > Mathf.Epsilon)
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            fieldOfViewDirection = Mathf.Repeat(turnSpeed <= 0f ? targetAngle
                : Mathf.MoveTowardsAngle(fieldOfViewDirection, targetAngle, turnSpeed * Mathf.Max(0f, deltaTime)), 360f);
        }
    }

    private Vector2 GetVisionDirection(float angleOffset)
    {
        float angle = (fieldOfViewDirection + angleOffset) * Mathf.Deg2Rad;
        return ((Vector2)transform.TransformDirection(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f))).normalized;
    }

    private float GetVisibleDistance(Vector2 direction, float length)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(visionObstacleLayers);
        filter.useTriggers = false;
        Physics2D.Raycast(VisionOrigin, direction, filter, visionHits, length);

        Transform playerTransform = TGameManager.Instance?.Player != null
            ? TGameManager.Instance.Player.transform : null;
        float visibleDistance = length;
        foreach (RaycastHit2D hit in visionHits)
        {
            Transform hitTransform = hit.collider.transform;
            if (hitTransform.IsChildOf(transform)
                || (playerTransform != null && hitTransform.IsChildOf(playerTransform)))
            {
                continue;
            }

            visibleDistance = Mathf.Min(visibleDistance, hit.distance);
        }

        return visibleDistance;
    }

    private void CreateVisionMesh()
    {
        Material material = Resources.Load<Material>("EnemyFieldOfView");
        if (material == null)
        {
            Debug.LogError("EnemyFieldOfView マテリアルが見つかりません。", this);
            return;
        }

        visionObject = new GameObject("Field of View");
        visionObject.layer = gameObject.layer;
        visionObject.transform.SetParent(transform, false);
        visionObject.transform.localPosition = visionOffset;
        visionRenderer = visionObject.AddComponent<MeshRenderer>();
        visionRenderer.sharedMaterial = material;
        visionRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        visionRenderer.receiveShadows = false;

        visionMesh = new Mesh { name = "Enemy Field of View" };
        visionMesh.MarkDynamic();
        visionMesh.vertices = visionVertices;
        int[] triangles = new int[VisionSegments * 3];
        for (int i = 0; i < VisionSegments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 2;
            triangles[i * 3 + 2] = i + 1;
        }
        visionMesh.triangles = triangles;
        visionObject.AddComponent<MeshFilter>().sharedMesh = visionMesh;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (wanderSettings != null && wanderSettings.enabled)
        {
            UnityEditor.Handles.color = Color.cyan;
            Vector3 origin = Application.isPlaying
                ? new Vector3(WanderOrigin.x, WanderOrigin.y, transform.position.z) : transform.position;
            UnityEditor.Handles.DrawWireDisc(origin, Vector3.forward, Mathf.Max(0f, wanderSettings.radius));
        }

        if (Application.isPlaying)
        {
            return;
        }

        Vector3 start = GetVisionDirection(-fieldOfViewAngle * 0.5f);
        UnityEditor.Handles.color = fieldOfViewColor;
        UnityEditor.Handles.DrawSolidArc(VisionOrigin, Vector3.forward, start,
            fieldOfViewAngle, fieldOfViewLength);
        UnityEditor.Handles.color = new Color(fieldOfViewColor.r, fieldOfViewColor.g, fieldOfViewColor.b, 1f);
        UnityEditor.Handles.DrawWireArc(VisionOrigin, Vector3.forward, start,
            fieldOfViewAngle, fieldOfViewLength);
        UnityEditor.Handles.DrawLine(VisionOrigin,
            VisionOrigin + (Vector3)GetVisionDirection(0f) * fieldOfViewLength);
    }
#endif

    public void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
        }
    }

    public void ChangeState(EnemyState newState)
    {
        Movement?.ResetPath();
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
}
