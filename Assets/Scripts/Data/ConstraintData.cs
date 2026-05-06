using UnityEngine;

public enum ConstraintType
{
    ExactSlot,
    Orientation,
    ForbiddenSlot,
    StackOrder
}
[System.Serializable]
public class ConstraintData
{
    public ConstraintType constraintType;

    public string objectA;
    public string objectB;

    public string slotID;

    public bool boolValue;
}
