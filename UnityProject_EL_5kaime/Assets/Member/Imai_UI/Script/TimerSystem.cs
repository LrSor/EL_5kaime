using UnityEngine;
using UnityEngine.Events;

public class TimerSystem : MonoBehaviour
{
    public static TimerSystem Instance { get; private set; }

    [SerializeField]public float MaxTime = 10f;
    private float currentTime = 0f;

    [SerializeField] UnityEvent unityEvent;

    private bool timeOverflg = false;

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
        currentTime = MaxTime;
        Debug.Log("タイマーのセット");
    }

    private void Update()
    {
        if (timeOverflg)
        {
            return;
        }
        currentTime -= Time.deltaTime;
        TimerUI.Instance.UpdateUI();



        if (currentTime <= 0)
        {
            TimeOverEvent();
            timeOverflg = true;
        }
    }

    public int GetTime()
    {
        return (int)currentTime;
    }
    
    /// <summary> 時間切れイベント </summary>
    public void TimeOverEvent()
    {
        unityEvent?.Invoke();
    }

    public void EventTest()
    {
        Debug.Log("時間切れイベント");
    }

    public void TimeMinus(float time)
    {
        currentTime -= time;
    }
}
