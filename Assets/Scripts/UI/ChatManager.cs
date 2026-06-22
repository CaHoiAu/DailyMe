using TMPro;
using UnityEngine;

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

    [Header("Unread Badges")]
    // Song song với contactNames/chatDetails — đặt badge số tin nhắn chưa đọc lên từng Contact row
    public TMP_Text[] unreadBadges;
    public UnityEngine.UI.Image[] unreadBadgesImg;

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

        RefreshUnreadBadges();
    }

    private void RefreshUnreadBadges()
    {
        if (unreadBadges == null) return;

        for (int i = 0; i < unreadBadges.Length; i++)
        {
            if (unreadBadges[i] == null) continue;

            int remaining = 0;
            if (i < chatDetails.Length && chatDetails[i] != null)
            {
                ChatSequence sequence = chatDetails[i].GetComponentInChildren<ChatSequence>(true);
                remaining = sequence != null ? sequence.RemainingMessages : 0;
            }

            unreadBadges[i].gameObject.SetActive(remaining > 0);
            unreadBadges[i].text = remaining.ToString();

            unreadBadgesImg[i].gameObject.SetActive(remaining > 0);
        }
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

        var highlight = FindObjectOfType<UIHighlights>();
        highlight?.StopHighlight();
    }

    public void BackFromDetail()
    {
        ShowContactList();
    }
}
