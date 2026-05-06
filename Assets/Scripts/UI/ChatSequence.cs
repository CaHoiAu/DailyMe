using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatSequence : MonoBehaviour
{
    [Header("UI References")]
    public ScrollRect scrollRect;
    public RectTransform messageContent;
    public GameObject messageRowPrefab;

    [Header("Chat Messages")]
    [TextArea]
    public List<string> messages;

    private int currentMessageIndex = 0;

    private void Start()
    {
    }

    public void ShowNextMessage()
    {
        if (currentMessageIndex >= messages.Count)
            return;
        GameObject row = Instantiate(messageRowPrefab, messageContent);
        TMP_Text text = row.GetComponentInChildren<TMP_Text>();
        text.text = messages[currentMessageIndex];
        text.ForceMeshUpdate();
        currentMessageIndex++;
        StartCoroutine(ScrollToBottomNextFrame());
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        foreach (RectTransform child in messageContent)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(child);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(messageContent);

        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ResetChat()
    {
        currentMessageIndex = 0;
        foreach (Transform child in messageContent)
        {
            Destroy(child.gameObject);
        }
    }
}
