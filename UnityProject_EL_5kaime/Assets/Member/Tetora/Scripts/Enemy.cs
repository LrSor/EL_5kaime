using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 100;
    public EnemyState currentState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        ChangeState(new EnemyChaseState(this));
    }

    // Update is called once per frame
    public void Update()
    {
        currentState?.Update();
    }
        
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
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
}
