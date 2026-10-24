using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState 
    {
    NORMAL,
    SIXSEVEN
    }

    [SerializeField] float m_moveSoeed;
    [SerializeField] Animator m_animator;

    private SpriteRenderer sightRenderer;

    public Vector3 FeetPosition => transform.position;

    // 追跡は足元、視界の検知は現在の画像の中央を使う。
    public Vector3 SightTargetPosition
    {
        get
        {
            if (sightRenderer == null)
            {
                sightRenderer = m_animator != null
                    ? m_animator.GetComponent<SpriteRenderer>() : GetComponentInChildren<SpriteRenderer>();
            }
            return sightRenderer != null ? sightRenderer.bounds.center : FeetPosition;
        }
    }

    PlayerState m_state;

    float m_horizontalInput;
    float m_verticalInput;

    string m_currentAnimation = "";

    void Start()
    {
        m_state = PlayerState.NORMAL;

    }

    void Update()
    {
        StateHandler();
    }

    private void PlayerMovement() 
    {
        m_horizontalInput = Input.GetAxisRaw("Horizontal");
        m_verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(m_horizontalInput, m_verticalInput);

        HorizontalDirectionChanger(moveDirection.x);

        if (moveDirection.x != 0)
        {
            ChangeAnimation("Horizontal");
        }
        else if (moveDirection.y > 0)
        {
            ChangeAnimation("Up");
        }
        else if (moveDirection.y < 0)
        {
            ChangeAnimation("Down");
        }
        else
        {
            ChangeAnimation("Idle");
        }

        transform.position += moveDirection * m_moveSoeed * Time.deltaTime;
    }

    private void HorizontalDirectionChanger(float moveDirectionX) 
    {
        if (moveDirectionX > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (moveDirectionX < 0) 
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void DoSixSeven() 
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            m_state = PlayerState.SIXSEVEN;
            ChangeAnimation("SixSeven");
        }
    }

    private void ExitSixSeven() 
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            m_state = PlayerState.NORMAL;
            ChangeAnimation("Idle");
        }
    }

    private void StateHandler() 
    {
        switch (m_state) 
        {
            case PlayerState.NORMAL:

                PlayerMovement();

                DoSixSeven();

                break;

            case PlayerState.SIXSEVEN:

                ExitSixSeven();

                break;

        }
    }

    private void ChangeAnimation(string animationName)
    {
        // 同じアニメーションなら何もしない
        if (m_currentAnimation == animationName)
        {
            return;
        }

        // 前のTriggerをリセット
        if (!string.IsNullOrEmpty(m_currentAnimation))
        {
            m_animator.ResetTrigger(m_currentAnimation);
        }

        m_animator.SetTrigger(animationName);

        m_currentAnimation = animationName;
    }

    public PlayerState GetPlayerState() 
    {
        return m_state;
    }
}
