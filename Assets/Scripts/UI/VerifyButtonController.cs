using System.Collections.Generic;
using UnityEngine;

public class VerifyButtonController : MonoBehaviour
{
    public GameObject verifyButton;
    public GameObject nextMessageButton;
    public List<ChatSequence> chatSequences; // ← assign tất cả ChatSequence vào

    private HashSet<ChatSequence> completedSequences = new HashSet<ChatSequence>();

    void Start()
    {
        // Ẩn button lúc đầu
        if (verifyButton != null)
            verifyButton.SetActive(false);
    }

    // Gọi từ onAllMessagesShown của từng ChatSequence
    public void OnSequenceCompleted(ChatSequence sequence)
    {
        if (completedSequences.Contains(sequence)) return;

        completedSequences.Add(sequence);
        Debug.Log($"[VerifyButton] {completedSequences.Count}/{chatSequences.Count} sequences completed");

        // ✅ Chỉ hiện button khi TẤT CẢ sequence đã xong
        if (completedSequences.Count >= chatSequences.Count)
        {
            verifyButton.SetActive(true);
            nextMessageButton.SetActive(false);
            Debug.Log("✅ All sequences done → Show verify button!");
        }
    }

    public void ResetAll()
    {
        completedSequences.Clear();
        if (verifyButton != null)
            verifyButton.SetActive(false);
        if (nextMessageButton != null)
            nextMessageButton.SetActive(true);
    }
}