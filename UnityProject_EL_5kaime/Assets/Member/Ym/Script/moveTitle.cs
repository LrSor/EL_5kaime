using UnityEngine;

public class moveTitle : MonoBehaviour
{
    [SerializeField] float moveHeight = 20f; // 上下する幅
    [SerializeField] float speed = 2f;       // 動く速さ
    [SerializeField] bool isMove = true;

    RectTransform rectTransform;
     Vector2 startPosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (!isMove)
            return;

        float y = Mathf.Sin(Time.time * speed) * moveHeight;

        rectTransform.anchoredPosition =
            startPosition + new Vector2(0, y);
    }
}
