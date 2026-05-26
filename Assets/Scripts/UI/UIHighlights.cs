using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIHighlights : MonoBehaviour
{
    [Header("Overlay Settings")]
    public Image overlayImage;
    public Color overlayColor = new Color(1f, 1f, 0f, 0.5f); // Yellow with 50% opacity

    [Header("Pulse Settings")]
    public float minAlpha = 0.4f;
    public float maxAlpha = 0.8f;
    public float pulseSpeed = 1f;

    private Graphic graphic;
    private Coroutine pulseCoroutine;
    private Color originalColor;

    void Awake()
    {
        graphic = GetComponent<Graphic>();
        if (graphic != null)
            originalColor = graphic.color;

        // ❌ Xóa dòng SetActive(false) ở đây
    }

    public void StartHighlight()
    {
        Debug.Log($"StartHighlight called! overlayImage={overlayImage?.name}");

        StopHighlight();

        // ✅ Chỉ SetActive(true) khi bắt đầu highlight
        if (overlayImage != null)
            overlayImage.gameObject.SetActive(true);

        pulseCoroutine = StartCoroutine(Pulse());
    }

    public void StopHighlight()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        // ✅ Chỉ ẩn khi stop
        if (overlayImage != null)
            overlayImage.gameObject.SetActive(false);

        if (graphic != null)
            graphic.color = originalColor;
    }
    private IEnumerator Pulse()
    {
        if (overlayImage != null)
        {
            overlayImage.gameObject.SetActive(true);
        }
        while (true)
        {
            float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, alpha);
            yield return null;
        }
    }
}
