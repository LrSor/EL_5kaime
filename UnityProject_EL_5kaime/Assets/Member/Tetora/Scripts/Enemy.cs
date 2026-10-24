using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 100;
    public EnemyState currentState;

    public float detectionGauge = 0f;

    public float detectionRange = 5f;

    [SerializeField]
    private GameObject detectionMeter = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        detectionGauge = 0f;
        ChangeState(new EnemyNormalState(this));
    }

    // Update is called once per frame
    public void Update()
    {
        currentState?.Update();
        detectionMeter.transform.localScale = new Vector3(detectionGauge, 0.1f, 1f);
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
