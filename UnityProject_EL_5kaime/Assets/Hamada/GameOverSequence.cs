using UnityEngine;

public class GameOverSequence : MonoBehaviour
{
    [SerializeField] PlayerController playerController;



    public void GameOver() 
    {
        playerController.GameOver();
    }
}
