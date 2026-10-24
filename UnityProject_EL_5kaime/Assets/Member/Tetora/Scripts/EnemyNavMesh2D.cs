using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Unity標準のNavMeshをXY平面へ回転して使用。追加パッケージや3D用Colliderは不要。
[DisallowMultipleComponent]
public sealed class EnemyNavMesh2D : MonoBehaviour
{
    [Tooltip("歩けるタイル。未指定なら同じシーンのFloorを自動選択")]
    public Tilemap floor;
    [Min(0.1f)] public float cellSize = 0.25f;
    [Min(0.1f)] public float obstacleCheckInterval = 1f;
    [Tooltip("床タイルがないシーンで使う最小生成範囲")]
    public Bounds fallbackBounds = new Bounds(Vector3.zero, new Vector3(40f, 30f, 0f));

    public int Version { get; private set; }
    public bool IsReady => data != null && dataInstance.valid;

    private readonly List<Collider2D> queryHits = new List<Collider2D>();
    private readonly HashSet<Collider2D> obstacles = new HashSet<Collider2D>();
    private NavMeshData data;
    private NavMeshDataInstance dataInstance;
    private float radius = 0.35f;
    private int obstacleLayers;
    private int geometryHash;
    private float nextObstacleCheck;
    private bool dirty = true;
    private int agentTypeID;

    public static EnemyNavMesh2D GetOrCreate(EnemyMovement2D requester)
    {
        Scene scene = requester.gameObject.scene;
        foreach (var surface in FindObjectsByType<EnemyNavMesh2D>(FindObjectsSortMode.None))
        {
            if (surface.gameObject.scene == scene && surface.isActiveAndEnabled) return surface;
        }
        var navigation = new GameObject("Enemy Navigation 2D");
        SceneManager.MoveGameObjectToScene(navigation, scene);
        return navigation.AddComponent<EnemyNavMesh2D>();
    }

    private void OnEnable()
    {
        dirty = true;
        Tilemap.tilemapTileChanged += OnTilesChanged;
    }

    private void OnDisable()
    {
        Tilemap.tilemapTileChanged -= OnTilesChanged;
        RemoveData();
    }

    private void OnTilesChanged(Tilemap changed, Tilemap.SyncTile[] tiles)
    {
        if (changed == floor) dirty = true;
    }

    public void EnsureReady(EnemyMovement2D requester)
    {
        float requiredRadius = requester.Radius + requester.collisionSkin * 2f;
        if (requiredRadius > radius || (obstacleLayers | requester.ObstacleLayers) != obstacleLayers)
        {
            radius = Mathf.Max(radius, requiredRadius);
            obstacleLayers |= requester.ObstacleLayers;
            dirty = true;
        }
        if (dirty || Version == 0) Rebuild();
    }

    private void Update()
    {
        nextObstacleCheck -= Time.deltaTime;
        if (nextObstacleCheck > 0f) return;
        nextObstacleCheck = obstacleCheckInterval;
        Physics2D.SyncTransforms();
        int hash = CollectObstacles();
        if (dirty || hash != geometryHash) Rebuild();
    }

    [ContextMenu("Rebuild 2D NavMesh")]
    public void Rebuild()
    {
        if (!isActiveAndEnabled) return;
        if (floor == null)
        {
            foreach (var tilemap in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            {
                if (tilemap.gameObject.scene == gameObject.scene
                    && string.Equals(tilemap.name, "Floor", System.StringComparison.OrdinalIgnoreCase))
                {
                    floor = tilemap;
                    break;
                }
            }
        }
        foreach (var tiles in FindObjectsByType<TilemapCollider2D>(FindObjectsSortMode.None))
        {
            if (tiles.gameObject.scene == gameObject.scene && tiles.hasTilemapChanges) tiles.ProcessTilemapChanges();
        }
        Physics2D.SyncTransforms();
        geometryHash = CollectObstacles();
        Bounds bounds = GetBounds();
        float resolution = Mathf.Max(0.1f, cellSize);
        // 過大な範囲でも起動時の処理量と頂点数を制限する。
        resolution = Mathf.Max(resolution, Mathf.Sqrt(bounds.size.x * bounds.size.y / 200000f));
        int width = Mathf.CeilToInt(bounds.size.x / resolution);
        int height = Mathf.CeilToInt(bounds.size.y / resolution);
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var filter = new ContactFilter2D();
        filter.SetLayerMask(obstacleLayers);
        filter.useTriggers = false;
        for (int y = 0; y < height; y++)
        {
            int stripStart = -1;
            for (int x = 0; x <= width; x++)
            {
                Vector2 center = new Vector2(bounds.min.x + (x + 0.5f) * resolution,
                    bounds.min.y + (y + 0.5f) * resolution);
                bool walkable = x < width && (floor == null || floor.HasTile(floor.WorldToCell(center)));
                if (walkable)
                {
                    Physics2D.OverlapBox(center, Vector2.one * resolution, 0f, filter, queryHits);
                    foreach (var hit in queryHits)
                    {
                        if (obstacles.Contains(hit)) { walkable = false; break; }
                    }
                }
                if (walkable && stripStart < 0) stripStart = x;
                if (!walkable && stripStart >= 0)
                {
                    AddStrip(vertices, triangles, bounds.min.x + stripStart * resolution,
                        bounds.min.x + x * resolution, bounds.min.y + y * resolution, resolution);
                    stripStart = -1;
                }
            }
        }

        var mesh = new Mesh { name = "Enemy Walkable Cells", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(0);
        agentTypeID = settings.agentTypeID;
        settings.agentRadius = radius;
        settings.agentHeight = 0.5f;
        settings.agentClimb = 0.05f;
        settings.minRegionArea = 0f;
        settings.overrideVoxelSize = true;
        settings.voxelSize = Mathf.Min(resolution / 2f, radius / 3f);
        var sources = new List<NavMeshBuildSource>
        {
            new NavMeshBuildSource { shape = NavMeshBuildSourceShape.Mesh, sourceObject = mesh,
                transform = Matrix4x4.identity, area = 0 }
        };
        var buildBounds = new Bounds(new Vector3(bounds.center.x, 0f, -bounds.center.y),
            new Vector3(bounds.size.x + 2f, 4f, bounds.size.y + 2f));
        RemoveData();
        if (triangles.Count > 0)
        {
            data = NavMeshBuilder.BuildNavMeshData(settings, sources, buildBounds, Vector3.zero, Quaternion.identity);
            if (data != null)
                dataInstance = NavMesh.AddNavMeshData(data, Vector3.zero, Quaternion.Euler(90f, 0f, 0f));
        }
        DestroyGenerated(mesh);
        dirty = false;
        Version++;
    }

    public bool CalculatePath(Vector2 start, Vector2 target, NavMeshPath path)
    {
        if (!IsReady) return false;
        var filter = new NavMeshQueryFilter { agentTypeID = agentTypeID, areaMask = NavMesh.AllAreas };
        float sampleDistance = Mathf.Max(cellSize * 2f, radius);
        if (!NavMesh.SamplePosition(new Vector3(start.x, start.y, 0f), out var from, sampleDistance, filter)
            || !NavMesh.SamplePosition(new Vector3(target.x, target.y, 0f), out var to, sampleDistance, filter)) return false;
        return NavMesh.CalculatePath(from.position, to.position, filter, path)
            && path.status == NavMeshPathStatus.PathComplete;
    }

    private int CollectObstacles()
    {
        obstacles.Clear();
        int hash = 0;
        foreach (var collider in FindObjectsByType<Collider2D>(FindObjectsSortMode.None))
        {
            if (collider.gameObject.scene != gameObject.scene || !collider.enabled || collider.isTrigger
                || (obstacleLayers & (1 << collider.gameObject.layer)) == 0
                || collider.GetComponentInParent<Enemy>() != null
                || collider.GetComponentInParent<PlayerController>() != null
                || (floor != null && collider.gameObject == floor.gameObject)
                || collider.compositeOperation != Collider2D.CompositeOperation.None) continue;
            obstacles.Add(collider);
            unchecked { hash += collider.GetInstanceID() ^ collider.bounds.GetHashCode() ^ (int)collider.GetShapeHash(); }
        }
        return hash;
    }

    private Bounds GetBounds()
    {
        if (floor != null && floor.GetComponent<TilemapRenderer>() != null)
        {
            var result = floor.GetComponent<TilemapRenderer>().bounds;
            if (result.size.x > 0f && result.size.y > 0f) return result;
        }
        Bounds bounds = fallbackBounds;
        foreach (var obstacle in obstacles) bounds.Encapsulate(obstacle.bounds);
        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            if (enemy.gameObject.scene != gameObject.scene) continue;
            bounds.Encapsulate(new Bounds(enemy.transform.position,
                Vector3.one * Mathf.Max(2f, enemy.wanderSettings.radius * 2f)));
        }
        bounds.Expand(2f);
        return bounds;
    }

    private static void AddStrip(List<Vector3> vertices, List<int> triangles, float left, float right, float bottom, float size)
    {
        int index = vertices.Count;
        vertices.Add(new Vector3(left, 0f, -bottom));
        vertices.Add(new Vector3(right, 0f, -bottom));
        vertices.Add(new Vector3(right, 0f, -bottom - size));
        vertices.Add(new Vector3(left, 0f, -bottom - size));
        triangles.Add(index); triangles.Add(index + 1); triangles.Add(index + 2);
        triangles.Add(index); triangles.Add(index + 2); triangles.Add(index + 3);
    }

    private void RemoveData()
    {
        if (dataInstance.valid) dataInstance.Remove();
        if (data != null) DestroyGenerated(data);
        data = null;
    }

    private static void DestroyGenerated(Object generated)
    {
        if (Application.isPlaying) Destroy(generated);
        else DestroyImmediate(generated);
    }
}
