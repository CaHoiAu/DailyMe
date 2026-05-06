using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ConstraintManager : MonoBehaviour
{
    public List<ICSPConstraint> constraints = new List<ICSPConstraint>();
    private void Awake()
    {
        constraints.Clear();
        constraints.AddRange(GetComponentsInChildren<ICSPConstraint>());
    }
    public void SetConstraints(List<ICSPConstraint> newConstraints)
    {
        constraints = newConstraints;
    }
    public bool CheckAllConstraints(GridDragObject[] objects)
    {
        Dictionary<string, PlacementValue> assignment = BuildAssignement(objects);
        foreach (var constraint in constraints)
        {
            if (!constraint.IsSatisfied(assignment))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsPlacementAllowed(GridDragObject obj, Vector2Int targetPosition, int rotationIndex, int stateIndex, int layer)
    {
        if (obj == null)
            return false;
        Dictionary<string, PlacementValue> hypotheticalAssignment = BuildAssignement(FindObjectsOfType<GridDragObject>());
        hypotheticalAssignment[obj.objectId] = new PlacementValue(targetPosition.x, targetPosition.y, rotationIndex, stateIndex, layer);
        foreach (var constraint in constraints)
        {
            if (constraint is CSPExactSlotConstraint exact)
            {
                if (exact.objectId == obj.objectId)
                {
                    Vector2Int assignedPos = new Vector2Int(hypotheticalAssignment[obj.objectId].x, hypotheticalAssignment[obj.objectId].y);
                    foreach (var pos in exact.requiredPosition)
                    {
                        if (assignedPos == pos)
                        {
                            return true; // Exact slot satisfied
                        }
                    }
                }
            }
        }
        return true;
    }
    private Dictionary<string, PlacementValue> BuildAssignement(GridDragObject[] objects)
    {
        Dictionary<string, PlacementValue> assignment = new Dictionary<string, PlacementValue>();
        foreach (var obj in objects)
        {
            if (obj == null || !obj.isPlaced) continue;

            int layer = obj.GetComponent<ILayerProvider>()?.GetLayer() ?? 0;
            assignment[obj.objectId] = new PlacementValue(
                obj.currentGridPosition.x,
                obj.currentGridPosition.y,
                obj.currentRotationIndex,
                obj.currentStateIndex,
                layer
                );
        }
        return assignment;
    }
    public List<string> GetViolatedConstraints()
    {
        Dictionary<string, PlacementValue> assignment = new Dictionary<string, PlacementValue>();
        List<string> violated = new List<string>();
        foreach (var constraint in constraints)
        {
            if (!constraint.IsSatisfied(assignment))
            {
                violated.Add(constraint.GetErrorMessage());
            }
        }
        return violated;
    }
    
    public void AddLayerOverlapConstraint(Dictionary<string, Vector2Int[]> objectShapes)
    {
        var layerConstraint = new CSPGridLayerOverlapConstraint(objectShapes);
        constraints.Add(layerConstraint);
    }
    
    public void ReplaceNonoOverlapWithLayerOverlap(Dictionary<string, Vector2Int[]> objectShapes)
    {
        constraints.RemoveAll(c => c is CSPNonOverlapConstraint);
        AddLayerOverlapConstraint(objectShapes);
    }
    public interface ILayerProvider
    {
        int GetLayer();
    }
}
