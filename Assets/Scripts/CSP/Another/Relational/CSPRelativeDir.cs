using System.Collections.Generic;
using UnityEngine;

public enum RelativeDirection
{
    Above,
    Down,
    LeftOf,
    RightOf,
}
public class CSPRelativeDir : ICSPConstraint
{
    private string objIdA, objIdB;
    private RelativeDirection direction;
    
    public CSPRelativeDir(string objIdA, string objIdB, RelativeDirection direction) : base(new List<string> { objIdA, objIdB })
    {
        this.objIdA = objIdA;
        this.objIdB = objIdB;
        this.direction = direction;
    }

    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
        if (!assignment.ContainsKey(objIdA) || !assignment.ContainsKey(objIdB))
        {
            return true; // If either variable is not assigned, we can't say it's violated
        }

        var posA = assignment[objIdA];
        var posB = assignment[objIdB];

        return direction switch
        {
            RelativeDirection.Above => posA.y > posB.y,
            RelativeDirection.Down => posA.y < posB.y,
            RelativeDirection.LeftOf => posA.x < posB.x,
            RelativeDirection.RightOf => posA.x > posB.x,
            _ => false
        };
    }
}
