using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[System.Serializable]
public class LandingZoneData
{
    public string zoneId; 
    public string[] objectId;
    public GameObject prefab;
    public Vector3 position;
    public Sprite spriteVisual;
    public Color colorVisual = Color.white;
}

[CreateAssetMenu(fileName = "GridMiniLevelData", menuName = "Level/Grid Mini Level Data")]
public class GridMiniLevelData : BaseMiniLevelData
{
    [Header("Board")]
    public int boardWidth = 5;
    public int boardHeight = 4;
    public float cellSize = 1f;
    public Vector3 boardOrigin;

    [Header("Objects")]
    public GridObjectData[] objects;

    [Header("Constraints")]
    public bool useBoundary = true;
    public bool useNonOverlap = true;
    public bool useFullCoverage = true;
    public bool useAllObjects = true;
    public bool useExactSlots = false;
    public bool useLayerOverlap = false;
    public SlotAssignment[] exactSlotAssignments = new SlotAssignment[0];

    [Header("Drop targets")]
    public bool useDropTargets = false;
    public DropTarget[] dropTargets = new DropTarget[0];

    [Header("Landing Zones")]
    public bool useLandingZones = false;
    public LandingZoneData[] landingZones = new LandingZoneData[0];

    [Header("Background")]
    public Sprite backgroundSprite;
    public Color backgroundColor = Color.white;
}
[System.Serializable]
public class SlotAssignment
{
    public string objectId;
    public Vector2Int[] slotPosition;
}
[System.Serializable]
public class DropTarget
{
    public string objectId;
    public float dropTargetPosition;
}