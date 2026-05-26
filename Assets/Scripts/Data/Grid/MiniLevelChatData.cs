using UnityEngine;

[CreateAssetMenu(menuName = "Chat/MiniLevelChatData")]
public class MiniLevelChatData : ScriptableObject
{
    public GridMiniLevelData miniLevelData;        // mini level tương ứng
    public ChatSequenceData[] contactSequences;    // mỗi contact 1 sequence
}