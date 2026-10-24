using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class TGameManager
{
    public static TGameManager Instance { get; private set; }

    public PlayerController Player { get; private set; }

    private TGameManager() { }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize()
    {
        Instance = new TGameManager();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        Instance.Player =
            Object.FindAnyObjectByType<PlayerController>();
    }
}
