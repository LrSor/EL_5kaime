using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Enemy))]
public sealed class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField, Min(0f)] private float minimumMoveSpeed = 0.01f;

    private Enemy enemy;
    private Vector3 previousPosition;
    private string currentAnimation = "";

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        if (spriteRenderer == null && animator != null)
        {
            spriteRenderer = animator.GetComponent<SpriteRenderer>();
        }
    }

    private void OnEnable()
    {
        previousPosition = transform.position;
        currentAnimation = "";
    }

    private void LateUpdate()
    {
        // 徘徊・追跡の移動が終わってから、実際に動いた方向で切り替える。
        Vector3 displacement = transform.position - previousPosition;
        previousPosition = transform.position;
        if (animator == null || spriteRenderer == null || Time.deltaTime <= 0f)
        {
            return;
        }

        Vector2 velocity = transform.InverseTransformDirection(displacement) / Time.deltaTime;
        string animationName = "Idle";
        bool flipX = false;
        if (!(enemy.currentState is EnemyDeadState)
            && velocity.sqrMagnitude > minimumMoveSpeed * minimumMoveSpeed)
        {
            // プレイヤーと同じく、斜め移動では横歩きを優先する。
            if (Mathf.Abs(velocity.x) > minimumMoveSpeed)
            {
                animationName = "Horizontal";
                flipX = velocity.x > 0f;
            }
            else
            {
                animationName = velocity.y > 0f ? "Up" : "Down";
            }
        }

        // 本体を反転すると視界の向きやゲージも変わるため、画像だけ反転する。
        spriteRenderer.flipX = flipX;
        ChangeAnimation(animationName);
    }

    private void ChangeAnimation(string animationName)
    {
        if (currentAnimation == animationName)
        {
            return;
        }

        if (!string.IsNullOrEmpty(currentAnimation))
        {
            animator.ResetTrigger(currentAnimation);
        }
        animator.SetTrigger(animationName);
        currentAnimation = animationName;
    }
}
