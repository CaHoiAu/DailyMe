using UnityEngine;

[System.Serializable]
public class MiniGameEntry
{
    public MiniLevelType miniLevelType;
    public ScriptableObject miniGameData;
    [Header("Chat")]
    public ChatSequenceData[] contactSequences;

    [Header("Cutscene (played before this mini-game loads)")]
    public CutsceneData cutscene;
}
