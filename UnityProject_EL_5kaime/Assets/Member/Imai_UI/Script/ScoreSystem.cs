using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }

    private int score = 0;

    private void Awake()
    {
        // シングルトン化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 初期化
        score = 0;
    }

    /// <summary> スコア加算 </summary>
    public void ScoreAdd(int num)
    {
        score += num;

        ScoreUI.Instance.UpdateUI();
    }

    public void ScoreMinus(int num)
    {
        score -= num;

        ScoreUI.Instance.UpdateUI();
    }

    public int GetScore()
    {
        return score;
    }
}
