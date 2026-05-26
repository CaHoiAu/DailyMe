// ChatLevelData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Chat/ChatLevelData")]
public class ChatLevelData : ScriptableObject
{
    public ChatSequenceData[] sequenceDatas; // mỗi contact 1 sequence
}