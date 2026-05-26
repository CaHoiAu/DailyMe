using System.Collections.Generic;
using UnityEngine;

public class CSPFullCoverageConstraint : ICSPConstraint
{
    private Dictionary<string, GridObjectState[]> objectStates;
    private int boardWidth;
    private int boardHeight;

    public CSPFullCoverageConstraint(Dictionary<string, GridObjectState[]> objectStates, int boardWidth, int boardHeight)
        : base(new List<string>())
    {
        this.objectStates = objectStates;
        this.boardWidth = boardWidth;
        this.boardHeight = boardHeight;
    }

    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
       var covered = new HashSet<Vector2Int>();

        foreach (var pair in assignment)
        {
            var objectName = pair.Key;
            var placement = pair.Value;
            if (!objectStates.ContainsKey(objectName))
            {
                continue;
            }
            var states = objectStates[objectName];
            if (placement.stateIndex >= states.Length)
            {
                continue;
            }
            Vector2Int[] baseShape = states[placement.stateIndex].cells;
            Vector2Int[] rotatedShape = ShapeUtils.RotateShape(baseShape, placement.rotation);
            Vector2Int[] translatedShape = ShapeUtils.TranslateCells(rotatedShape, placement.x, placement.y);

            foreach (var cell in translatedShape)
            {
                covered.Add(cell);
            }
        }
        return covered.Count == boardWidth * boardHeight;
    }
}