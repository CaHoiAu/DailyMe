using System.Collections.Generic;

public class CSPRequireStateOnlyConstraint : ICSPConstraint
{
    private string objectId;
    private int requiredStateIndex;
    private GridDragObject[] dragObjects;

    public CSPRequireStateOnlyConstraint(
        string objectId, int requiredStateIndex,
        GridDragObject[] dragObjects)
        : base(new List<string> { objectId })
    {
        this.objectId = objectId;
        this.requiredStateIndex = requiredStateIndex;
        this.dragObjects = dragObjects;
    }

    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
        // ✅ Tìm thẳng object trong scene, không qua assignment
        var obj = System.Array.Find(dragObjects, o => o.objectId == objectId);
        if (obj == null) return false;

        return obj.currentStateIndex == requiredStateIndex;
    }
}