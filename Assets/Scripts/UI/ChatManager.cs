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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowContactList();
    }
    public void ShowContactList()
    {
        contactListPanel.SetActive(true);
        foreach (GameObject panel in chatDetails)
        {
            panel.SetActive(false);
        }
        headerDetail.SetActive(false);
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
    }
    public void BackFromDetail()
    {
        ShowContactList();
    }
}
