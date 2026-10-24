using System.Collections;
using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI countText;

    private IEnumerator Start()
    {
        countText.gameObject.SetActive(true);

        yield return ShowCount("3", Color.red);
        yield return ShowCount("2", new Color(1f, 0.5f, 0f));
        yield return ShowCount("1", Color.yellow);
        yield return ShowCount("START!", Color.green, 1.5f);

        countText.gameObject.SetActive(false);

        GameManager.Instance.GameStart();
    }

    private IEnumerator ShowCount(string text, Color color, float duration = 1f)
    {
        countText.text = text;
        countText.color = color;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float rate = timer / duration;
            float scale = Mathf.Lerp(2.5f, 1f, rate);
            countText.transform.localScale = Vector3.one * scale;

            yield return null;
        }
    }
}
