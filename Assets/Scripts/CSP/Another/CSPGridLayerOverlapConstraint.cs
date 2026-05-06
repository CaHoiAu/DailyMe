using System.Collections.Generic;
using UnityEngine;

public class CSPGridLayerOverlapConstraint : ICSPConstraint
{
    private Dictionary<string, Vector2Int[]> objectShapes;
    public CSPGridLayerOverlapConstraint(Dictionary<string, Vector2Int[]> objectShapes): base(new List<string>(objectShapes.Keys))
    {
        this.objectShapes = objectShapes;
    }
    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
        //Dictionary: cell, Dictionary<int, List<string>: Layer
        Dictionary<Vector2Int, Dictionary<int, List<string>>> cellLayerMap = new Dictionary<Vector2Int, Dictionary<int, List<string>>>();
        foreach (var pair in assignment)
        {
            string objectId = pair.Key;
            PlacementValue value = pair.Value;

            if (!objectShapes.ContainsKey(objectId))
            {
                Debug.LogError($"Object ID {objectId} not found in objectShapes.");
                continue;
            }

            Vector2Int[] shape = objectShapes[objectId];
            Vector2Int[] rotatedShape = ShapeUtils.RotateShape(shape, value.rotation);
            Vector2Int[] occupied = ShapeUtils.TranslateCells(rotatedShape, value.x, value.y);

            foreach (var cell in occupied)
            {
                if (!cellLayerMap.ContainsKey(cell))
                {
                    cellLayerMap[cell] = new Dictionary<int, List<string>>();
                }
                if (!cellLayerMap[cell].ContainsKey(value.layer))
                {
                    cellLayerMap[cell][value.layer] = new List<string>();
                }
                if (cellLayerMap[cell][value.layer].Count > 0)
                {
                    // Layer đã có object khác chiếm, vi phạm constraint
                    return false;
                }
                cellLayerMap[cell][value.layer].Add(objectId);
            }
        }
        return true;
    }
}
