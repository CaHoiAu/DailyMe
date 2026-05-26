// LevelChatData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Chat/LevelChatData")]
public class LevelChatData : ScriptableObject
{
    public MiniLevelChatData[] miniLevels; // danh sách mini level theo thứ tự
}