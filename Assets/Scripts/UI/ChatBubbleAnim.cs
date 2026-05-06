using UnityEngine;
using System.Collections;

public class ChatBubbleAnim : MonoBehaviour
{
    public float duration = 0.25f;
    public Vector2 startOffset = new Vector2(0, -20f);

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Vector2 targetPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        StartCoroutine(PlayAnim());
    }

    IEnumerator PlayAnim()
    {
        targetPos = rect.anchoredPosition;

        rect.anchoredPosition = targetPos + startOffset;
        canvasGroup.alpha = 0f;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            t = Mathf.SmoothStep(0, 1, t);

            rect.anchoredPosition = Vector2.Lerp(targetPos + startOffset, targetPos, t);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        rect.anchoredPosition = targetPos;
        canvasGroup.alpha = 1f;
    }
}