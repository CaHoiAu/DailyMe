using System.Collections.Generic;
using UnityEngine;

public class CSPNonOverlapConstraint : ICSPConstraint
{
    private Dictionary<string, Vector2Int[]> objectShapes;

    public CSPNonOverlapConstraint(Dictionary<string, Vector2Int[]> objectShapes)
        : base(new List<string>())
    {
        this.objectShapes = objectShapes;
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

            Vector2Int[] baseShape = objectShapes[objectId];
            Vector2Int[] rotatedShape = ShapeUtils.RotateShape(baseShape, value.rotation);
            Vector2Int[] occupied = ShapeUtils.TranslateCells(rotatedShape, value.x, value.y);

            foreach (var cell in occupied)
            {
                if (usedCells.Contains(cell))
                {
                    return false; // Overlap detected
                }
                usedCells.Add(cell);
            }
        }

        return true;
    }
}