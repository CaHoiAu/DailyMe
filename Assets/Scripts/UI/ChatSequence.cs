using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChatSequence : MonoBehaviour
{
    [Header("UI References")]
    public ScrollRect scrollRect;
    public RectTransform messageContent;
    public GameObject messageRowPrefab;

    [Header("Chat Messages")]
    [TextArea] public List<string> messages;

    [Header("Conditions - cùng index với messages")]
    public List<ObjectStateCondition> conditions; // index trùng với messages
    // -1 ở requiredStateId = không cần condition

    [Header("Level Data Reference")]
    public BaseMiniLevelData miniLevelData;

    private int currentMessageIndex = 0;
    private bool waitingForCondition = false;

    private GridDragObject[] cachedObjects;

    [Header("Events")]
    public UnityEvent onWaitingForConditon;
    public UnityEvent onReadyForNext;
    public UnityEvent onAllMessageShown;

    private bool allMessagesShown = false;

    [Header("Data")]
    public ChatSequenceData sequenceData; // ← assign asset vào đây

    //[Header("Onboarding")]
    //public OnBoardingHighLight onBoardingHighLight;
    private List<string> GetAllObjectIds()
    {
        GridMiniLevelData gridData = miniLevelData as GridMiniLevelData;
        if (gridData == null) return new List<string>(); List<string> objectIds = new List<string>();
        foreach (var objData in gridData.objects)
        {
            if (!string.IsNullOrEmpty(objData.objectId))
                objectIds.Add(objData.objectId);
        }
        return objectIds;
    }
    private void Start()
    {
        if (sequenceData != null)
            LoadData(sequenceData);
        cachedObjects = FindObjectsByType<GridDragObject>(FindObjectsSortMode.None);
    }
    public void LoadData(ChatSequenceData data)
    {
        if (data == null) return;

        messages = data.messages;
        conditions = data.conditions;
        miniLevelData = data.miniLevelData;

        // Cache lại objects
        cachedObjects = FindObjectsByType<GridDragObject>(FindObjectsSortMode.None);

        // Reset chat
        ResetChat();

        allMessagesShown = false;
        waitingForCondition = false;

        Debug.Log($"[ChatSequence] Loaded {messages.Count} messages");
    }
    public void ShowNextMessage()
    {
        if (currentMessageIndex >= messages.Count)
        {
            if (!allMessagesShown)
            {
                allMessagesShown = true;
                onAllMessageShown?.Invoke();
                NotifyCompleted();
            }
            return;
        }

        if (waitingForCondition)
        {
            return;
        }
        var cond = GetCondition(currentMessageIndex);
        if (cond != null && cond.isLevelBreak)
        {
            // Dừng lại ở đây — chờ level mới load xong
            Debug.Log($"[ChatSequence] Level break at message {currentMessageIndex}: {cond.levelBreakNote}");
            onAllMessageShown?.Invoke();
            NotifyCompleted();
            return;
        }
        SpawnBubble(messages[currentMessageIndex]);
        ApplyLockSettings(currentMessageIndex);

        if (cond != null && !string.IsNullOrEmpty(cond.objectId) && cond.requiredStateId != -1)
        {
            waitingForCondition = true;
            onWaitingForConditon?.Invoke();
        }
        else
        {
            onReadyForNext?.Invoke();
        }

        currentMessageIndex++;
        StartCoroutine(ScrollToBottomNextFrame());
    }
    public void ResumeFromLevelBreak()
    {
        allMessagesShown = false;
        waitingForCondition = false;

        // Cache lại objects mới của level mới
        cachedObjects = FindObjectsByType<GridDragObject>(FindObjectsSortMode.None);

        Debug.Log($"[ChatSequence] Resuming from message {currentMessageIndex}");

        // Auto-complete if no messages remain (e.g. mini level has no chat)
        if (messages == null || messages.Count == 0 || currentMessageIndex >= messages.Count)
        {
            allMessagesShown = true;
            onAllMessageShown?.Invoke();
            NotifyCompleted();
        }
    }
    public void NotifyCompleted()
    {
        var verifyController = FindObjectOfType<VerifyButtonController>();
        verifyController?.OnSequenceCompleted(this);
    }
    void Update()
    {
        if (!waitingForCondition) return;

        var cond = GetCondition(currentMessageIndex - 1);

        if (cond == null || string.IsNullOrEmpty(cond.objectId) || cond.requiredStateId == -1)
        {
            Debug.Log("[Update] ✅ No condition → ready");
            waitingForCondition = false;
            onReadyForNext?.Invoke();
            return;
        }

        // ✅ Log mỗi 60 frames thôi (không spam)
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Update] ⏳ Still waiting for condition:");
            Debug.Log($"  Object: {cond.objectId}");
            Debug.Log($"  Required State: {cond.requiredStateId}");

            var obj = GetObjectById(cond.objectId);
            if (obj != null)
                Debug.Log($"  Current State: {obj.CurrentStateId}");
            else
                Debug.Log($"  ❌ Object NOT FOUND");
        }

        if (CheckObjectState(cond))
        {
            Debug.Log("[Update] ✅✅✅ Condition MET → ready!");
            waitingForCondition = false;
            if (currentMessageIndex >= messages.Count)
            {
                // No more messages — complete immediately without requiring another tap
                if (!allMessagesShown)
                {
                    allMessagesShown = true;
                    onAllMessageShown?.Invoke();
                    NotifyCompleted();
                }
            }
            else
            {
                onReadyForNext?.Invoke();
            }
        }
    }

    ObjectStateCondition GetCondition(int index)
    {
        if (conditions == null || index >= conditions.Count) return null;
        return conditions[index];
    }

    bool CheckObjectState(ObjectStateCondition condition)
    {
        if (string.IsNullOrEmpty(condition.objectId))
        {
            Debug.Log("[CheckObjectState] ✅ No objectId required");
            return true;
        }

        foreach (var obj in cachedObjects)
        {
            if (obj.objectId == condition.objectId)
            {
                bool isMatch = obj.CurrentStateId == condition.requiredStateId;

                // ✅ Chi tiết hơn
                Debug.Log($"[CheckObjectState] Checking '{obj.objectId}':");
                Debug.Log($"  Type of obj: {obj.GetType().Name}");
                Debug.Log($"  CurrentStateId: {obj.CurrentStateId} (type: {obj.CurrentStateId.GetType().Name})");
                Debug.Log($"  requiredStateId: {condition.requiredStateId} (type: {condition.requiredStateId.GetType().Name})");
                Debug.Log($"  Are they equal? {isMatch}");

                if (!isMatch)
                {
                    // Kiểm tra xem object có states không
                    if (obj.states != null)
                        Debug.Log($"  Object has {obj.states.Length} states");
                    else
                        Debug.Log($"  ⚠️ Object has NO states!");
                }

                return isMatch;
            }
        }

        Debug.LogWarning($"[CheckObjectState] ❌ Object '{condition.objectId}' NOT FOUND in cached objects ({cachedObjects.Length} total)!");
        foreach (var obj in cachedObjects)
            Debug.LogWarning($"  - {obj.objectId}");

        return false;
    }

    private void SpawnBubble(string message)
    {
        GameObject row = Instantiate(messageRowPrefab, messageContent);
        TMP_Text text = row.GetComponentInChildren<TMP_Text>();
        text.text = message;
        text.ForceMeshUpdate();
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        foreach (RectTransform child in messageContent)
            LayoutRebuilder.ForceRebuildLayoutImmediate(child);
        LayoutRebuilder.ForceRebuildLayoutImmediate(messageContent);
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void ApplyLockSettings(int messageIndex)
    {
        var cond = GetCondition(messageIndex);

        // Build danh sách bị lock
        HashSet<string> keepLocked = new HashSet<string>();
        if (cond?.excludeFromUnlock != null)
            foreach (var id in cond.excludeFromUnlock)
                keepLocked.Add(id);

        // Apply toàn bộ object trong level
        List<string> allIds = GetAllObjectIds();
        foreach (var id in allIds)
        {
            var obj = GetObjectById(id);
            if (obj == null) continue;
            obj.isLocked = keepLocked.Contains(id);
        }
    }
    private void LockAll()
    {
        List<string> allIds = GetAllObjectIds();
        foreach (var id in allIds)
        {
            var obj = GetObjectById(id);
            if (obj == null)
            {
                Debug.LogWarning($"Object with ID {id} not found for unlocking.");
                continue;
            }
            obj.isLocked = true;
        }
    }
    private GridDragObject GetObjectById(string objectId)
    {
        // ✅ Refresh cache nếu trống (objects chưa được tạo lúc Start)
        if (cachedObjects == null || cachedObjects.Length == 0)
        {
            cachedObjects = FindObjectsByType<GridDragObject>(FindObjectsSortMode.None);
            Debug.Log($"[GetObjectById] Cache was empty, refreshed to {cachedObjects.Length} objects");
        }

        foreach (var obj in cachedObjects)
        {
            if (obj != null && obj.objectId == objectId)
            {
                return obj;
            }
        }

        // ✅ Nếu vẫn không tìm được, refresh lần nữa (dynamic spawn?)
        cachedObjects = FindObjectsByType<GridDragObject>(FindObjectsSortMode.None);
        Debug.LogWarning($"[GetObjectById] Object '{objectId}' not found, refreshed cache to {cachedObjects.Length} objects");

        foreach (var obj in cachedObjects)
        {
            if (obj != null && obj.objectId == objectId)
            {
                Debug.Log($"[GetObjectById] ✅ Found after refresh!");
                return obj;
            }
        }

        Debug.LogError($"[GetObjectById] ❌ Object '{objectId}' still NOT FOUND!");
        return null;
    }
    public void ResetChat()
    {
        currentMessageIndex = 0;
        waitingForCondition = false;
        allMessagesShown = false; 
        foreach (Transform child in messageContent)
            Destroy(child.gameObject);
    }
}