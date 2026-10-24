using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState 
    {
    NORMAL,
    SIXSEVEN
    }

    [SerializeField] float m_moveSoeed;

    PlayerState m_state;

    float m_horizontalInput;
    float m_verticalInput;

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

        transform.position += moveDirection * m_moveSoeed * Time.deltaTime;
    }

    private void DoSixSeven() 
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            m_state = PlayerState.SIXSEVEN;
        }
    }

    private void ExitSixSeven() 
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            m_state = PlayerState.NORMAL;
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

    public PlayerState GetPlayerState() 
    {
        return m_state;
    }
}
