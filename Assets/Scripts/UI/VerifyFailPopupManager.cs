using System.Collections;
using UnityEngine;

public class VerifyFailPopupManager : MonoBehaviour
{
    public static VerifyFailPopupManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject popupRoot;

    [Header("Settings")]
    public float autoHideDelay = 1.5f; // <= 0 = không tự ẩn, chờ người chơi bấm tắt

    private Coroutine hideCoroutine;

    void Awake()
    {
        Instance = this;
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    public void Show()
    {
        if (popupRoot == null) return;

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        popupRoot.SetActive(true);

        if (autoHideDelay > 0f)
            hideCoroutine = StartCoroutine(AutoHide());
    }

    public void Hide()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(autoHideDelay);
        popupRoot.SetActive(false);
        hideCoroutine = null;
    }
}
