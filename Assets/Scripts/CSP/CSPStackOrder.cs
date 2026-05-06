using UnityEngine;

public class CSPStackOrder : ICSPConstraint
{
    private string lowerObjectId;
    private string upperObjectId;

    public CSPStackOrder(string lowerObjectId, string upperObjectId) : base(new System.Collections.Generic.List<string> { lowerObjectId, upperObjectId })
    {
        this.lowerObjectId = lowerObjectId;
        this.upperObjectId = upperObjectId;
    }
    public override bool IsSatisfied(System.Collections.Generic.Dictionary<string, PlacementValue> assignment)
    {
        //if (!assignment.ContainsKey(lowerObjectId) || !assignment.ContainsKey(upperObjectId)) return false;
        //if (assignment[lowerObjectId].slotId != assignment[upperObjectId].slotId) return false; // If they are not in the same slot, the order doesn't matter
        //return assignment[lowerObjectId].layer < assignment[upperObjectId].layer;
        return true;
    }
}
