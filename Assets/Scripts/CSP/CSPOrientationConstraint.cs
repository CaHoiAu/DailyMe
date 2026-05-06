using System.Collections.Generic;
using UnityEngine;

public class CSPOrientationConstraint : ICSPConstraint
{
    public string ObjectId { get; private set; }
    public bool MustBeVertical { get; private set; }

    public CSPOrientationConstraint(string objectId, bool mustBeVertical): base(new List<string> { objectId })
    {
        ObjectId = objectId;
        MustBeVertical = mustBeVertical;
    }

    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
        if (!assignment.ContainsKey(ObjectId)) return false;

        //return assignment[ObjectId].isVertical == MustBeVertical;
        return true;
    }
}
