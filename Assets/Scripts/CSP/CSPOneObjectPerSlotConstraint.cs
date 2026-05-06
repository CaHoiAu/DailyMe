using System.Collections.Generic;
using UnityEngine;

public class CSPOneObjectPerSlotConstraint : ICSPConstraint
{
    public CSPOneObjectPerSlotConstraint() : base(new List<string>())
    {
    }
    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
        //HashSet<string> usedPositions = new HashSet<string>();

        //foreach (var pair in assignment)
        //{
        //    PlacementValue value = pair.Value;
        //    string key = value.slotId + "_" + value.layer;

        //    if (usedPositions.Contains(key))
        //        return false;

        //    usedPositions.Add(key);
        //}

        return true;
    }
}
