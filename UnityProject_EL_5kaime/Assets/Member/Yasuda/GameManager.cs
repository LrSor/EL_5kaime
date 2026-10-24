using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Ready,
        Playing,
        GameOver,
    }
    public GameState State { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        State = GameState.Ready;
    }

    public void SetState(GameState state)
    {
        Debug.Log($"GameState : {State} -> {state}");

        State = state;
    }

    public void ResetGame()
    {
        State = GameState.Ready;
    }

    public void GameStart()
    {
        State = GameState.Playing;
    }

    public void GameOver()
    {
        if (State == GameState.GameOver)
            return;

        State = GameState.GameOver;
    }

    public bool IsPlaying()
    {
        return State == GameState.Playing;
    }

    public bool IsGameOver()
    {
        return State == GameState.GameOver;
    }
}
