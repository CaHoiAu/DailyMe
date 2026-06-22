using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LoadingScreenManager : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;

    [Header("Timing")]
    public float fadeDuration = 0.3f;
    public float minDisplayDuration = 5f; // thời gian tối thiểu hiện loading, tránh chớp tắt quá nhanh

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
    }

    public void Show(Action onContentReady)
    {
        gameObject.SetActive(true);
        StartCoroutine(ShowRoutine(onContentReady));
    }

    private IEnumerator ShowRoutine(Action onContentReady)
    {
        canvasGroup.alpha = 0f;
        yield return Fade(0f, 1f);
        yield return new WaitForSeconds(minDisplayDuration);

        // Dựng nội dung level mới trong khi màn hình loading vẫn đang che hoàn toàn
        onContentReady?.Invoke();

        yield return Fade(1f, 0f);
        gameObject.SetActive(false);
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
