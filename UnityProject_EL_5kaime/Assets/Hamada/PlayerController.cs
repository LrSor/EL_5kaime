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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
    }

    private void PlayerMovement() 
    {
        m_horizontalInput = Input.GetAxisRaw("Horizontal");
        m_verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(m_horizontalInput, m_verticalInput);

        transform.position += moveDirection * m_moveSoeed * Time.deltaTime;
    }

    public PlayerState GetPlayerState() 
    {
        return m_state;
    }
}
