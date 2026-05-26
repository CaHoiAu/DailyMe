using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public GameObject contactListPanel;
    public GameObject[] chatDetails;
    public ChatButtonController chatButtonController;

    public GameObject headerDetail;
    public TMP_Text headerName;
    public UnityEngine.UI.Image headerImage;

    public string[] contactNames;
    public Sprite[] contactImages;

    private PhonePanelManager phonePanelManager; // ✅ Reference để lock/unlock tabs

    void Start()
    {
        ShowContactList();
        // ✅ Tìm PhonePanelManager
        phonePanelManager = FindObjectOfType<PhonePanelManager>();
    }

    public void ShowContactList()
    {
        contactListPanel.SetActive(true);
        foreach (GameObject panel in chatDetails)
        {
            panel.SetActive(false);
        }
        headerDetail.SetActive(false);

        // ✅ Mở khóa tabs khi quay lại danh sách
        if (phonePanelManager != null)
            phonePanelManager.UnlockAllTabs();
    }

    public void OpenContact(int index)
    {
        ChatSequence chatSequence = chatDetails[index].GetComponentInChildren<ChatSequence>(true);

        contactListPanel.SetActive(false);
        for (int i = 0; i < chatDetails.Length; i++)
        {
            bool isActive = (i == index);
            chatDetails[i].SetActive(isActive);
        }
        headerDetail.SetActive(true);
        headerName.text = contactNames[index];
        headerImage.sprite = contactImages[index];

        chatButtonController.SetActiveChat(chatSequence);

        // ✅ Khóa tất cả tabs khi mở chat
        if (phonePanelManager != null)
            phonePanelManager.LockAllTabs();

        var highlight = FindObjectOfType<UIHighlights>();
        highlight?.StopHighlight();
    }

    public void BackFromDetail()
    {
        ShowContactList();
    }
}
