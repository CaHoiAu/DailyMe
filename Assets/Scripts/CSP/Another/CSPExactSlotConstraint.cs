using System.Collections.Generic;
using UnityEngine;

public class CSPExactSlotConstraint : ICSPConstraint
{
    public string objectId { get; private set; }
    public Vector2Int[] requiredPosition {get; private set; }

    public CSPExactSlotConstraint(string objectId, Vector2Int[] requiredPosition) : base(new List<string>() { objectId })
    {
        this.objectId = objectId;
        this.requiredPosition= requiredPosition;
    }

    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
        if (!assignment.ContainsKey(objectId)) return true;

        Vector2Int assignedPos = new Vector2Int(assignment[objectId].x, assignment[objectId].y);

        foreach (var pos in requiredPosition)
        {
            if (assignedPos == pos)
            {
                return true;
            }
        }
        return false;
    }
}
