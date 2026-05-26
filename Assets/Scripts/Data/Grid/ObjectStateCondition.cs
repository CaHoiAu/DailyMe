using UnityEngine;

[System.Serializable]
public class ObjectStateCondition
{
    public string objectId;
    public int requiredStateId;
    public int messageIndex; // index của message cần condition này, để debug

    [Header("Lock Settings")]
    public string[] excludeFromUnlock;

    [Header("Level Break")]
    public bool isLevelBreak = false;  // ← đánh dấu kết thúc level
    public string levelBreakNote;
}