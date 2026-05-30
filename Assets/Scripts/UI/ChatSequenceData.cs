using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChatSequenceData", menuName = "Chat/ChatSequenceData")]
public class ChatSequenceData : ScriptableObject
{
    [TextArea] public List<string> messages;
    public List<ObjectStateCondition> conditions;
    public BaseMiniLevelData miniLevelData;
}
