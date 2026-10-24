using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public static ScoreUI Instance { get; private set; }

    [SerializeField] private TMP_Text numberText;
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
        UpdateUI();
    }

    public void UpdateUI()
    {
        numberText.text = ScoreSystem.Instance.GetScore().ToString("D6");
    }
}
