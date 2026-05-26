using System.Collections.Generic;
using UnityEngine;

public class CSPNonOverlapConstraint : ICSPConstraint
{
    private Dictionary<string, GridObjectState[]> objectStates;

    public CSPNonOverlapConstraint(Dictionary<string, GridObjectState[]> objectStates)
        : base(new List<string>())
    {
        this.objectStates = objectStates;
    }

    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
        var layerCells = new Dictionary<int, HashSet<(int, int)>>();

        foreach (var pair in assignment)
        {
            var objectId = pair.Key;
            var placement = pair.Value;
            if (!objectStates.ContainsKey(objectId))
            {
                continue;
            }
            var states = objectStates[objectId];
            if (placement.stateIndex >= states.Length)
            {
                continue;
            }

            Vector2Int[] baseShape = states[placement.stateIndex].cells;
            Vector2Int[] rotatedShape = ShapeUtils.RotateShape(baseShape, placement.rotation);
            Vector2Int[] translatedShape = ShapeUtils.TranslateCells(rotatedShape, placement.x, placement.y);

            int layer = placement.layer;
            if (!layerCells.ContainsKey(layer))
            {
                layerCells[layer] = new HashSet<(int, int)>();
            }

            foreach (var cell in translatedShape)
            {
                var cellPos = (cell.x, cell.y);
                if (layerCells[layer].Contains(cellPos))
                {
                    return false; // Overlap detected
                }
                layerCells[layer].Add(cellPos);
            }
        }
        return true; // No overlaps detected
    }
}