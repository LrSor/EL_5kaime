using SceneFlow;
using UnityEngine;

public class GameOverSequence : MonoBehaviour
{
    [SerializeField] PlayerController playerController;

    [SerializeField] SceneReference m_scene;

    float m_loadTime;

    bool m_isLoading;

    bool m_canLoad;


    public void GameOver() 
    {
        playerController.GameOver();

        SceneTransition.Go(m_scene);
    }
}
