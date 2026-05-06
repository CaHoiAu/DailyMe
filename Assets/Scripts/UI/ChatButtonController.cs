using UnityEngine;

public class ChatButtonController : MonoBehaviour
{
    private ChatSequence chatSequence;

    public void SetActiveChat(ChatSequence sequence)
    {
        if (sequence == null)
        {
            Debug.LogError("ChatSequence is null");
            return;
        }

        chatSequence = sequence;
        Debug.Log("Active chat set: " + chatSequence.name);
    }
    public void ShowNextMessage()
    {
        Debug.Log("Next button clicked");

        if (chatSequence == null)
        {
            Debug.LogWarning("No active chat selected");
            return;
        }

        chatSequence.ShowNextMessage();
    }
}
