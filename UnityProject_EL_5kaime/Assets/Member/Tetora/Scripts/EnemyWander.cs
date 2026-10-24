using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyWander
{
    private const float ArrivalDistance = 0.05f;
    private const float WallMargin = 0.02f;
    private readonly Enemy enemy;
    private readonly List<RaycastHit2D> hits = new List<RaycastHit2D>();
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

        Vector2 direction = offset / distance;
        float moveTime = Mathf.Min(deltaTime, remainingTime);
        float step = Mathf.Min(distance, settings.speed * moveTime);
        float allowedStep = GetAllowedStep(direction, step, settings);
        if (allowedStep > 0f)
        {
            Vector3 position = enemy.transform.position;
            enemy.FaceTowards(position + (Vector3)direction, settings.turnSpeed,
                Mathf.Min(moveTime, allowedStep / settings.speed));
            enemy.transform.position = position + (Vector3)(direction * allowedStep);
        }

        remainingTime -= deltaTime;
        if (allowedStep < step || distance - allowedStep <= ArrivalDistance || remainingTime <= 0f)
        {
            BeginWait();
        }
    }

    private void PickDestination()
    {
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

    private float GetAllowedStep(Vector2 direction, float step, EnemyWanderSettings settings)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(settings.obstacleLayers);
        filter.useTriggers = false;
        if (settings.collisionRadius > 0f)
        {
            Physics2D.CircleCast(enemy.transform.position, settings.collisionRadius, direction,
                filter, hits, step + WallMargin);
        }
        else
        {
            Physics2D.Raycast(enemy.transform.position, direction, filter, hits, step + WallMargin);
        }

        float allowedStep = step;
        foreach (RaycastHit2D hit in hits)
        {
            if (!hit.collider.transform.IsChildOf(enemy.transform))
            {
                allowedStep = Mathf.Min(allowedStep, Mathf.Max(0f, hit.distance - WallMargin));
            }
        }
        return allowedStep;
    }
}
