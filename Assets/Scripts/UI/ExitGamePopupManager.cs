using UnityEngine;

// Hiện popup xác nhận thoát game khi bấm phím Esc hoặc nút thoát trên UI.
public class ExitGamePopupManager : MonoBehaviour
{
    public static ExitGamePopupManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject popupRoot;

    void Awake()
    {
        Instance = this;
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Show();
    }

    public void Show()
    {
        if (popupRoot == null) return;
        popupRoot.SetActive(true);
    }

    public void Hide()
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    // Gán cho nút "Đồng ý" / "Có" trên popup.
    public void ConfirmExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Gán cho nút "Hủy" / "Không" trên popup.
    public void CancelExit()
    {
        Hide();
    }
}