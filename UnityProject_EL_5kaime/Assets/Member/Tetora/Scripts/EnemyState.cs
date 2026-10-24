using UnityEngine;

public abstract class EnemyState
{
    protected Enemy Enemy { get; }
    protected PlayerController Player => TGameManager.Instance.Player;

    protected EnemyState(Enemy enemy)
    {
        Enemy = enemy;
    }

    public virtual void Enter()
    {
    }
    public virtual void Update()
    {
        // Default implementation does nothing
    }
    public virtual void FixedUpdate()
    {
        // Default implementation does nothing
    }
    public virtual void Exit()
    {
    }
}
