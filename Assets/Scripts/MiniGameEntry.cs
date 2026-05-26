using UnityEngine;

[System.Serializable]
public class MiniGameEntry
{
    public MiniLevelType miniLevelType;
    public ScriptableObject miniGameData;
    [Header("Chat")]
    public ChatSequenceData[] contactSequences;
}
