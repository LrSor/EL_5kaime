using Miyashita.Audio;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState 
    {
    NORMAL,
    SIXSEVEN,
    GAMEOVER,
    STAN
    }

    [SerializeField] float m_moveSoeed;
    [SerializeField] Animator m_animator;

    [SerializeField, Tooltip("画像の足元を示すTransform。Visualの位置をずらしても追跡先が一致する。未指定なら本体の原点")]
    private Transform feetAnchor;

    [SerializeField]
    private Disco.DiscoBeat m_discoBeat;

    private SpriteRenderer sightRenderer;

    public Vector3 FeetPosition => feetAnchor != null ? feetAnchor.position : transform.position;

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

    float m_stanTime;

    string m_currentAnimation = "";

    void Start()
    {
        m_state = PlayerState.NORMAL;

    }

    void Update()
    {
        if (!GameManager.Instance.IsPlaying()) 
        {
            return;
        }

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

    public void Stan() 
    {
        m_state = PlayerState.STAN;
        ChangeAnimation("GameOver");
        m_stanTime = 0;
    }

    public void GameOver() 
    {
        m_state = PlayerState.GAMEOVER;
        ChangeAnimation("GameOver");
    }

    private void DoSixSeven() 
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            SoundManager.Instance.PlaySE("se.six_seven");
            m_state = PlayerState.SIXSEVEN;
            ChangeAnimation("SixSeven");
            m_discoBeat?.SetEffect(true);
        }
    }

    private void ExitSixSeven() 
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            m_state = PlayerState.NORMAL;
            ChangeAnimation("Idle");
            m_discoBeat?.SetEffect(false);
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

                //m_scoreUpTime += Time.deltaTime;

                //if (m_scoreUpTime > 1) 
                //{
                //    ScoreSystem.Instance.ScoreAdd(100);
                //    m_scoreUpTime = 0;
                //}

                ExitSixSeven();

                break;

            case PlayerState.GAMEOVER:

                break;

            case PlayerState.STAN:

                m_stanTime += Time.deltaTime;

                if (m_stanTime > 3) 
                {
                    m_state = PlayerState.NORMAL;
                    ChangeAnimation("Idle");
                    m_stanTime = 0;
                }

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
