using System.Collections.Generic;
using UnityEngine;

public class CSPGridLayerOverlapConstraint : ICSPConstraint
{
    private Dictionary<string, GridObjectState[]> objectShapes;
    public CSPGridLayerOverlapConstraint(Dictionary<string, GridObjectState[]> objectShapes): base(new List<string>(objectShapes.Keys))
    {
        this.objectShapes = objectShapes;
    }
    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
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
            Vector2Int[] baseShape = objectShapes[objectId][value.stateIndex].cells;
            Vector2Int[] rotatedShape = ShapeUtils.RotateShape(baseShape, value.rotation);
            Vector2Int[] occupied = ShapeUtils.TranslateCells(rotatedShape, value.x, value.y);

            foreach (var cell in occupied)
            {
                if (!cellLayerMap.TryGetValue(cell, out var layerDict))
                {
                    layerDict = new Dictionary<int, List<string>>();
                    cellLayerMap[cell] = layerDict;
                }

                if (!layerDict.TryGetValue(value.layer, out var objectList))
                {
                    objectList = new List<string>();
                    layerDict[value.layer] = objectList;
                }

                if (objectList.Count > 0)
                {
                    return false;
                }

                objectList.Add(objectId);
            }
        }
        
        return true;
    }
}
