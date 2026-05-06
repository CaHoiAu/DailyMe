using UnityEngine;
using UnityEngine.U2D;

public class CSPBoundaryContraint : ICSPConstraint
{
    private string objectID;
    private Vector2Int[] baseShape;
    private int boardWidth;
    private int boardHeight;

    public CSPBoundaryContraint(string objectID, Vector2Int[] baseShape, int boardWidth, int boardHeight) : base(new System.Collections.Generic.List<string>() { objectID })
    {
        this.objectID = objectID;
        this.baseShape = baseShape;
        this.boardWidth = boardWidth;
        this.boardHeight = boardHeight;
    }

    public override bool IsSatisfied(System.Collections.Generic.Dictionary<string, PlacementValue> assignment)
    {
        if (!assignment.ContainsKey(objectID))
            return true;
        PlacementValue value = assignment[objectID];
        Vector2Int[] roatedShape = ShapeUtils.RotateShape(baseShape, value.rotation);
        Vector2Int[] occupied = ShapeUtils.TranslateCells(roatedShape, value.x, value.y);
        foreach (var cell in occupied)
        {
            if (cell.x < 0 || cell.x >= boardWidth || cell.y < 0 || cell.y >= boardHeight)
            {
                return false;
            }
        }
        return true;
    }
}
