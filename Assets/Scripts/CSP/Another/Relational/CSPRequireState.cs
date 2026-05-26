using System.Collections.Generic;
using UnityEngine;

public class CSPRequireState : ICSPConstraint
{
    private string objId;
    private int requiredStateIndex;

    public CSPRequireState(string objId, int requiredStateIndex) : base(new List<string> { objId})
    {
        this.objId = objId;
        this.requiredStateIndex = requiredStateIndex;
    }
    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
        if (!assignment.ContainsKey(objId)) return true;
        return assignment[objId].stateIndex == requiredStateIndex;
    }
}
