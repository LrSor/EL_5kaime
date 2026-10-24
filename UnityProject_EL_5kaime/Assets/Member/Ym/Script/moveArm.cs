using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class moveArm : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform moveImage;
    public float moveSpeed = 1.0f;

    public Vector2 movePos;
    Vector2 startPos;
    Vector2 targetPos;

    private void Start()
    {
        startPos = moveImage.anchoredPosition;
    }

    

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("カーソルが入った");
        moveImage.anchoredPosition = startPos + movePos;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("カーソルが出た");
        moveImage.anchoredPosition = startPos;
    }
}
