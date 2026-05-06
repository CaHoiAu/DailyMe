using System.Collections.Generic;

public class CSPUseAllObjectsConstraint : ICSPConstraint
{
    private List<string> allObjectIds;

    public CSPUseAllObjectsConstraint(List<string> allObjectIds)
        : base(allObjectIds)
    {
        this.allObjectIds = allObjectIds;
    }

    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
        foreach (var id in allObjectIds)
        {
            if (!assignment.ContainsKey(id))
                return false;
        }

        return true;
    }
}