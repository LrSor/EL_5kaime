using UnityEngine;

public sealed class EnemyWander
{
    private const float ArrivalDistance = 0.05f;
    private readonly Enemy enemy;
    private Vector2 destination;
    private float remainingTime;
    private bool waiting;

    public EnemyWander(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Begin()
    {
        PickDestination();
    }

    public void Update(float deltaTime)
    {
        EnemyWanderSettings settings = enemy.wanderSettings;
        if (!settings.enabled || settings.speed <= 0f || settings.radius <= 0f || deltaTime <= 0f)
        {
            return;
        }

        destination = enemy.WanderOrigin + Vector2.ClampMagnitude(destination - enemy.WanderOrigin, settings.radius);

        if (waiting)
        {
            remainingTime -= deltaTime;
            if (remainingTime <= 0f)
            {
                PickDestination();
            }
            return;
        }

        if (remainingTime <= 0f)
        {
            BeginWait();
            return;
        }

        Vector2 offset = destination - (Vector2)enemy.transform.position;
        float distance = offset.magnitude;
        if (distance <= ArrivalDistance)
        {
            BeginWait();
            return;
        }

        float moveTime = Mathf.Min(deltaTime, remainingTime);
        Vector2 moved = enemy.Movement.MoveTowards(destination, settings.speed, moveTime);
        if (moved.sqrMagnitude > 0.000001f)
        {
            enemy.FaceTowards(enemy.transform.position + (Vector3)moved, settings.turnSpeed, moveTime);
        }

        remainingTime -= deltaTime;
        if (moved.sqrMagnitude <= 0.000001f
            || Vector2.Distance(destination, enemy.transform.position) <= ArrivalDistance || remainingTime <= 0f)
        {
            BeginWait();
        }
    }

    private void PickDestination()
    {
        enemy.Movement?.ResetPath();
        destination = enemy.WanderOrigin + Random.insideUnitCircle * Mathf.Max(0f, enemy.wanderSettings.radius);
        remainingTime = RandomDuration(enemy.wanderSettings.walkTimeRange);
        waiting = false;
    }

    private void BeginWait()
    {
        remainingTime = RandomDuration(enemy.wanderSettings.waitTimeRange);
        waiting = true;
    }

    private static float RandomDuration(Vector2 range)
    {
        float minimum = Mathf.Max(0f, Mathf.Min(range.x, range.y));
        float maximum = Mathf.Max(minimum, Mathf.Max(range.x, range.y));
        return Random.Range(minimum, maximum);
    }
}
