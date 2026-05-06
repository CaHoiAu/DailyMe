using System.Collections.Generic;
using UnityEngine;

public class CSPFullCoverageConstraint : ICSPConstraint
{
    private Dictionary<string, Vector2Int[]> objectShapes;
    private int boardWidth;
    private int boardHeight;

    public CSPFullCoverageConstraint(Dictionary<string, Vector2Int[]> objectShapes, int boardWidth, int boardHeight)
        : base(new List<string>())
    {
        this.objectShapes = objectShapes;
        this.boardWidth = boardWidth;
        this.boardHeight = boardHeight;
    }

    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
        HashSet<Vector2Int> usedCells = new HashSet<Vector2Int>();

        foreach (var pair in assignment)
        {
            string objectId = pair.Key;
            PlacementValue value = pair.Value;

            if (!objectShapes.ContainsKey(objectId))
                continue;

            Vector2Int[] shape = ShapeUtils.RotateShape(objectShapes[objectId], value.rotation);
            Vector2Int[] occupied = ShapeUtils.TranslateCells(shape, value.x, value.y);

            foreach (var cell in occupied)
            {
                usedCells.Add(cell);
            }
        }

        return usedCells.Count == boardWidth * boardHeight;
    }
}