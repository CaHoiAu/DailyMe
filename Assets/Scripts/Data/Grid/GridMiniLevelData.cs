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
    public Vector3 scale = Vector3.one;
    public Sprite spriteVisual;
    public Color colorVisual = Color.white;
    public int sortingOrder = 0;
    public bool isVisualOnly = false;
}
[System.Serializable]
public class AdjacentConstraintData
{
    public string objIdA;
    public string objIdB;
}
[System.Serializable]
public class RequireStateConstraintData
{
    public string objectId;
    public int requiredStateIndex; // index trong states[], không phải stateId
}

[System.Serializable]
public class RelativePositionConstraintData
{
    public string objectIdA;
    public RelativeDirection direction;
    public string objectIdB;
}

[System.Serializable]
public class BelowAdjacentConstraintData
{
    public string objectIdA; // A là object to
    public string objectIdB; // B phải nằm bên dưới A
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

    [Header("Partial Constraints")]
    public bool useBoundary = true;
    public bool useNonOverlap = true;
    public bool useFullCoverage = true;
    public bool useAllObjects = true;
    public bool useExactSlots = false;
    public bool useLayerOverlap = false;
    public SlotAssignment[] exactSlotAssignments = new SlotAssignment[0];

    [Header("Relational Constraints")]
    public AdjacentConstraintData[] adjacentConstraints = new AdjacentConstraintData[0];
    public RequireStateConstraintData[] requireStateConstraints = new RequireStateConstraintData[0];
    public RelativePositionConstraintData[] relativePositionConstraints = new RelativePositionConstraintData[0];
    public BelowAdjacentConstraintData[] belowAdjacentConstraints;

    [Header("Drop targets")]
    public bool useDropTargets = false;
    public DropTarget[] dropTargets = new DropTarget[0];

    [Header("Landing Zones")]
    public bool useLandingZones = false;
    public LandingZoneData[] landingZones = new LandingZoneData[0];

    [Header("Background")]
    public Sprite backgroundSprite;
    public Color backgroundColor = Color.white;
    public Vector3 backgroundScale = Vector3.one;
    public Vector3 backgroundPosition = Vector3.zero;
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