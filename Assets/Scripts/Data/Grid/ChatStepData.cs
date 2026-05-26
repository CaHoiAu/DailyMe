// ChatStepData.cs — dùng array thay List
using UnityEngine;

[System.Serializable]
public class ChatStepData
{
    public string message;
    public ObjectStateCondition[] conditions;
    public ConditionOperator conditionOperator = ConditionOperator.AND;

    public bool HasCondition()
    {
        return conditions != null && conditions.Length > 0;
    }
}