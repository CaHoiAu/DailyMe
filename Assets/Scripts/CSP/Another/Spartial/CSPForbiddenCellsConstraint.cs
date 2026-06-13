using System.Collections.Generic;
using UnityEngine;

public class CSPForbiddenCellsConstraint : ICSPConstraint
{
    private string objectID;
    private GridObjectState[] states;
    private Vector2Int[] forbiddenCells;

    public CSPForbiddenCellsConstraint(string objectID, GridObjectState[] states, Vector2Int[] forbiddenCells) : base(new List<string>() { objectID })
    {
        this.objectID = objectID;
        this.states = states;
        this.forbiddenCells = forbiddenCells;
    }

    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
        if (!assignment.ContainsKey(objectID))
            return true;

        PlacementValue placement = assignment[objectID];
        if (placement.stateIndex >= states.Length) return false;

        Vector2Int[] baseShape = states[placement.stateIndex].cells;
        Vector2Int[] rotatedShape = ShapeUtils.RotateShape(baseShape, placement.rotation);
        Vector2Int[] translatedShape = ShapeUtils.TranslateCells(rotatedShape, placement.x, placement.y);

        foreach (Vector2Int cell in translatedShape)
        {
            foreach (Vector2Int forbidden in forbiddenCells)
            {
                if (cell == forbidden)
                    return false;
            }
        }
        return true;
    }
}
