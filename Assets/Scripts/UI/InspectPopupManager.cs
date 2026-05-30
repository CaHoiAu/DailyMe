using UnityEngine;
using UnityEngine.UI;

public class InspectPopupManager : MonoBehaviour
{
    public static InspectPopupManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject popupRoot;
    public Image displayImage;
    public Button closeButton; // optional — assign overlay button or dedicated close button

    void Awake()
    {
        Instance = this;
        popupRoot.SetActive(false);
    }

    public void Show(Sprite sprite)
    {
        displayImage.sprite = sprite;
        popupRoot.SetActive(true);
    }

    public void Hide()
    {
        popupRoot.SetActive(false);
    }
}
