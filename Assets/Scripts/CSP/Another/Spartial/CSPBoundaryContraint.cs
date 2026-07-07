using UnityEngine;

public class CSPBoundaryContraint : ICSPConstraint
{
    private string objectID;
    private GridObjectState[] states;
    private int boardWidth;
    private int boardHeight;

    public CSPBoundaryContraint(string objectID, GridObjectState[] states, int boardWidth, int boardHeight) : base(new System.Collections.Generic.List<string>() { objectID })
    {
        this.objectID = objectID;
        this.states = states;
        this.boardWidth = boardWidth;
        this.boardHeight = boardHeight;
    }

    public override bool IsSatisfied(System.Collections.Generic.Dictionary<string, PlacementValue> assignment)
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
            if (cell.x < 0 || cell.x >= boardWidth || cell.y < 0 || cell.y >= boardHeight)
            {
                return false;
            }
        }
        return true;
    }
}
