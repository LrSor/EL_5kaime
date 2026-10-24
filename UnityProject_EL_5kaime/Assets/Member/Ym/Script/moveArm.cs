using Miyashita.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class moveArm : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] RectTransform moveImage;
    [SerializeField] float moveSpeed = 1.0f;
    [SerializeField] string seID;

    [SerializeField] Vector2 movePos;
    Vector2 startPos;
    Vector2 targetPos;

    bool isMove = false;



    void Start()
    {
        startPos = moveImage.anchoredPosition;
    }

    void Update()
    {
        if (!isMove)
            return;

        moveImage.anchoredPosition = Vector2.Lerp(
            moveImage.anchoredPosition,
            targetPos,
            moveSpeed * Time.deltaTime
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySE(seID);

        Debug.Log("カーソルが入った");
        //moveImage.anchoredPosition = startPos + movePos;
        targetPos = startPos + movePos;

        if (!isMove)
            isMove = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("カーソルが出た");
        moveImage.anchoredPosition = startPos;
        targetPos = startPos;

        if(isMove)
            isMove = false;
    }
}
