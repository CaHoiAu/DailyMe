using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GridPuzzleManager : MonoBehaviour
{
    public GridMiniLevelData miniLevelData;
    public GridBoardManager boardManager;
    public Transform puzzleContainer;
    public Transform landingZonesContainer;
    public LevelManager levelManager;
    public ConstraintManager constraintManager;

    private GridDragObject[] objects;
    private bool puzzleCompleted = false;
    private List<GridDragObject> spawnedObjects = new List<GridDragObject>();

    [Header("Drop Effect")]
    public float dropEffectDuration = 2f;
    public float dropEffectHeight = 2f;
    public AnimationCurve dropEffectCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Layer Settings")]
    public bool useLayerOverlap = false;
    public int maxLayers = 1;
    public void LoadPuzzle(GridMiniLevelData data)
    {
        Debug.Log("=== LOADING PUZZLE ===");
        Debug.Log($"Mini Level: {data.name}");

        if (data == null)
        {
            Debug.LogError("GridMiniLevelData is missing");
            return;
        }

        if (boardManager == null)
        {
            Debug.LogError("Board Manager is missing");
            return;
        }

        if (puzzleContainer == null)
        {
            Debug.LogError("Puzzle Container is missing");
            return;
        }

        ClearPuzzle();

        puzzleCompleted = false;
        miniLevelData = data;

        boardManager.SetUp(data);
        SpawnLandingZones(data);
        SpawnObjects(data);

        // ✅ THAY ĐỔI: Dùng spawnedObjects thay vì GetComponentsInChildren
        objects = spawnedObjects.ToArray();
        Debug.Log($"[LoadPuzzle] Total objects spawned: {objects.Length}");
        foreach (var obj in objects)
        {
            Debug.Log($"  - {obj.objectId}");
        }

        InitializeContainedObjects(data);

        foreach (var obj in objects)
        {
            if (obj != null)
            {
                obj.FinalizeRuntimeSetup();
            }
        }

        if (useLayerOverlap && constraintManager != null)
        {
            SetUpLayerConstraints();
        }

        Debug.Log("=== PUZZLE LOADED COMPLETE ===");
    }

    private void InitializeContainedObjects(GridMiniLevelData data)
    {
        Debug.Log("=== Initializing Contained Objects ===");
        Debug.Log($"Total objects in level data: {data.objects.Length}");

        foreach (var objData in data.objects)
        {
            GridDragObject containerObj = System.Array.Find(objects, o => o != null && o.objectId == objData.objectId);
            if (containerObj == null)
            {
                Debug.LogWarning($"❌ Container [{objData.objectId}] not found in scene!");
                continue;
            }

            Debug.Log($"\n[{containerObj.objectId}] - Checking states:");

            bool hasAnyContainerState = false;

            // ✅ Loop qua ALL states
            for (int i = 0; i < containerObj.states.Length; i++)
            {
                GridObjectState state = containerObj.states[i];
                int containedCount = state.containedObjectIds != null ? state.containedObjectIds.Length : 0;
                Debug.Log($"  State[{i}] ({state.stateName}): isContainer={state.isContainer}, containedIds={containedCount}");

                if (!state.isContainer || state.containedObjectIds == null || state.containedObjectIds.Length == 0)
                    continue;

                hasAnyContainerState = true;
                Debug.Log($"    ✅ Container state found!");

                foreach (var containedObjId in state.containedObjectIds)
                {
                    GridDragObject containedObj = System.Array.Find(objects, o => o != null && o.objectId == containedObjId);
                    if (containedObj == null)
                    {
                        Debug.LogError($"    ❌ Contained object [{containedObjId}] NOT FOUND!");
                        continue;
                    }

                    Debug.Log($"    ✅ Setting [{containedObjId}].currentContainer = [{containerObj.objectId}]");

                    containedObj.currentContainer = containerObj;
                    containedObj.isContainedObject = true;

                    containedObj.SetVisible(false);
                    Collider2D collider = containedObj.GetComponent<Collider2D>();
                    if (collider != null) collider.enabled = false;
                }
            }

            if (!hasAnyContainerState)
            {
                Debug.LogWarning($"  ⚠️ [{containerObj.objectId}] has NO container states!");
            }
        }

        Debug.Log("=== Containment Initialization Complete ===\n");
    }
    private void SetUpLayerConstraints()
    {
        var shapeDict = new Dictionary<string, Vector2Int[]>();
        foreach (var obj in objects){
            if (obj == null) continue;
            GridObjectState state = obj.GetCurrentState();
            if (state != null && state.cells != null)
            {
                shapeDict[obj.objectId] = state.cells;
            }
        }
        if (shapeDict.Count > 0)
        {
            constraintManager.ReplaceNonoOverlapWithLayerOverlap(shapeDict);
             Debug.Log("Layer overlap constraints set up with " + shapeDict.Count + " objects");
        }
    }

    private void ClearPuzzle()
    {
        // ✅ THÊM: Reset tất cả currentContainer references trước khi xóa
        GridDragObject[] allObjectsBeforeClear = puzzleContainer.GetComponentsInChildren<GridDragObject>();
        foreach (var obj in allObjectsBeforeClear)
        {
            if (obj != null)
            {
                obj.ResetToDefaultState();
            }
        }

        spawnedObjects.Clear();
        foreach (Transform child in puzzleContainer)
        {
            Destroy(child.gameObject);
        }
        if (landingZonesContainer != null)
        {
            foreach (Transform child in landingZonesContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void SpawnLandingZones(GridMiniLevelData data)
    {
        // ✅ Check if landing zones should be spawned
        if (!data.useLandingZones || data.landingZones == null || data.landingZones.Length == 0)
        {
            Debug.Log("Landing zones are disabled or empty");
            return;
        }

        if (landingZonesContainer == null)
        {
            GameObject containerObj = new GameObject("LandingZones");
            containerObj.transform.parent = transform;
            landingZonesContainer = containerObj.transform;
        }

        foreach (var zoneData in data.landingZones)
        {
            if (zoneData.prefab == null)
            {
                Debug.LogError($"Landing zone prefab is null for zone {zoneData.zoneId}");
                continue;
            }

            GameObject zone = Instantiate(zoneData.prefab, landingZonesContainer);
            zone.transform.position = zoneData.position;
            zone.transform.localScale = Vector3.one; // ✅ Reset scale
            zone.name = $"LandingZone_{zoneData.zoneId}";

            LandingZone landingZoneComponent = zone.GetComponent<LandingZone>();
            if (landingZoneComponent != null)
            {
                if (zoneData.spriteVisual != null)
                {
                    landingZoneComponent.SetVisuals(zoneData.spriteVisual, zoneData.colorVisual);
                    Debug.Log($"Landing zone {zoneData.zoneId} spawned with sprite from LandingZoneData");
                }
                else
                {
                    // ✅ Nếu không, gọi ApplyVisuals() để dùng sprite từ prefab
                    landingZoneComponent.ApplyVisuals();
                    Debug.Log($"Landing zone {zoneData.zoneId} spawned with sprite from prefab");
                }
            }
            else
            {
                Debug.LogWarning($"Landing zone {zoneData.zoneId}: LandingZone component not found");
            }
        }
    }

    public void TryAddObjectToContainer(GridDragObject container, GridDragObject objToAdd)
    {
        if (container == null || objToAdd == null) return;

        //if (!container.IsContainer())
        //{
        //    Debug.LogWarning($"{container.objectId} is not a container");
        //    return;
        //}

        //if (!container.CanAddMore())
        //{
        //    Debug.LogWarning($"{container.objectId} is full!");
        //    return;
        //}

        container.AddContainedObject(objToAdd);
        OnObjectChanged();
    }
    private void SpawnObjects(GridMiniLevelData data)
    {
        // ✅ First, collect all IDs that should be contained
        HashSet<string> containedObjectIds = new HashSet<string>();
        foreach (var objData in data.objects)
        {
            if (objData.states == null || objData.states.Length == 0) continue;

            for (int i = 0; i < objData.states.Length; i++)
            {
                var state = objData.states[i];
                if (state.isContainer && state.containedObjectIds != null)
                {
                    foreach (var containedId in state.containedObjectIds)
                    {
                        containedObjectIds.Add(containedId);
                    }
                }
            }
        }

        foreach (var objData in data.objects)
        {
            bool willBeContained = containedObjectIds.Contains(objData.objectId);

            // ✅ Spawn at start position (even if contained, will be hidden later)
            GameObject obj = Instantiate(objData.prefab, puzzleContainer);
            obj.transform.position = objData.startWorldPosition;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            GridDragObject drag = obj.GetComponent<GridDragObject>();
            if (drag == null)
            {
                Debug.LogError($"Prefab {objData.prefab.name} does not have GridDragObject!");
                continue;
            }

            drag.objectId = objData.objectId;
            drag.startWorldPosition = objData.startWorldPosition;  // ✅ Keep original start position
            drag.gridPuzzleManager = this;
            drag.SetLayer(objData.currentLayer);
            drag.SetDropEffectSettings(objData.dropDuration, objData.dropHeight, objData.dropCurve);

            // ✅ Mark as contained if it will be
            drag.isContainedObject = willBeContained;

            if (objData.states != null && objData.states.Length > 0)
            {
                drag.states = new GridObjectState[objData.states.Length];

                for (int i = 0; i < objData.states.Length; i++)
                {
                    drag.states[i] = new GridObjectState
                    {
                        stateName = objData.states[i].stateName,
                        sprite = objData.states[i].sprite,
                        cells = objData.states[i].cells,
                        allowRotate = objData.states[i].allowRotate,
                        isContainer = objData.states[i].isContainer,
                        containedObjectIds = objData.states[i].containedObjectIds,
                        containedObjectDisplays = objData.states[i].containedObjectDisplays
                    };
                }

                drag.currentStateIndex = 0;
                drag.currentRotationIndex = 0;

                spawnedObjects.Add(drag);
            }
            else
            {
                Debug.LogWarning($"Object {objData.objectId} has no states defined!");
            }
        }

        //objects = puzzleContainer.GetComponentsInChildren<GridDragObject>();
        objects = spawnedObjects.ToArray(); 

        // ✅ IMPORTANT: Now Initialize all objects after they are created but before containment logic
        foreach (var drag in objects)
        {
            if (drag != null)
            {
                drag.Initialize();
            }
        }
    }

    public bool CanKeepPlacement(GridDragObject obj, Vector2Int gridPos)
    {
        if (obj == null) return false;

        if (miniLevelData.useBoundary)
        {
            bool inside = GridConstraintChecker.CheckBoundary(boardManager, obj, gridPos);
            if (!inside) return false;
        }

        if (miniLevelData.useNonOverlap)
        {
            bool noOverlap = GridConstraintChecker.CheckNonOverlap(obj, gridPos, objects);
            if (!noOverlap) return false;
        }

        if (miniLevelData.useExactSlots)
        {
            // ✅ Pass the moving object and grid position
            bool exactSlots = GridConstraintChecker.CheckExactSlots(obj, gridPos, objects, miniLevelData.exactSlotAssignments);
            if (!exactSlots) return false;
        }

        if (miniLevelData.useLayerOverlap)
        {
            bool layerOverlap = GridConstraintChecker.CheckLayerOverlap(obj, gridPos, objects);
            if (layerOverlap) return false;
        }
        return true;
    }

    public bool TryPlaceObject(GridDragObject obj, Vector2Int gridPos)
    {
        if (obj == null) return false;

        if (miniLevelData.useBoundary)
        {
            bool inside = GridConstraintChecker.CheckBoundary(boardManager, obj, gridPos);
            if (!inside)
            {
                Debug.Log("Boundary constraint failed");
                return false;
            }
        }

        if (miniLevelData.useNonOverlap)
        {
            bool noOverlap = GridConstraintChecker.CheckNonOverlap(obj, gridPos, objects);
            if (!noOverlap)
            {
                Debug.Log("Non-overlap constraint failed");
                return false;
            }
        }

        if (miniLevelData.useExactSlots)
        {
            // ✅ Pass the moving object and grid position
            bool exactSlots = GridConstraintChecker.CheckExactSlots(obj, gridPos, objects, miniLevelData.exactSlotAssignments);
            if (!exactSlots)
            {
                Debug.Log("Exact slot constraint failed");
                return false;
            }
        }

        if (miniLevelData.useLayerOverlap)
        {
            bool layerOverlap = GridConstraintChecker.CheckLayerOverlap(obj, gridPos, objects);
            if (!layerOverlap)
            {
                Debug.Log("Layer overlap constraint failed!");
                return false;
            }
        }
        obj.SnapToGrid(gridPos);
        return true;
    }

    public void RemoveObjectFromBoard(GridDragObject obj)
    {
        if (obj == null) return;
        obj.isPlaced = false;
    }

    public void OnObjectChanged()
    {
    }
    private Vector3 GetDropTargetPosition(GridDragObject obj)
    {
        // Get the grid position where the object was placed
        Vector3 gridWorldPos = boardManager.GridToWorld(obj.currentGridPosition);

        // Account for shape center offset (same as SnapToGrid)
        Vector2Int[] shape = obj.GetCurrentShape();
        Vector2 shapeCenter = obj.GetShapeCenter(shape);
        Vector3 gridCenterPos = gridWorldPos + new Vector3(
            shapeCenter.x * boardManager.cellSize,
            shapeCenter.y * boardManager.cellSize,
            0f);

        // ✅ Use custom drop targets if enabled - only customize Y position
        if (miniLevelData.useDropTargets && miniLevelData.dropTargets != null)
        {
            foreach (var dropTarget in miniLevelData.dropTargets)
            {
                if (dropTarget.objectId == obj.objectId)
                {
                    // ✅ Keep X from grid calculation, use custom Y position
                    return new Vector3(
                        gridCenterPos.x,
                        dropTarget.dropTargetPosition,
                        gridCenterPos.z
                    );
                }
            }
        }

        return gridCenterPos;
    }
    private IEnumerator<WaitForSeconds> PlayAllDropEffects()
    {
        //collect all placed objects and play drop effect
        List<GridDragObject> placedObjects = new List<GridDragObject>();
        foreach (var obj in objects)
        {
            if (obj != null && obj.isPlaced)
            {
                Vector3 targetPos = GetDropTargetPosition(obj);
                obj.PlayDropEffect(targetPos);
                placedObjects.Add(obj);
            }
        }
        if (placedObjects.Count == 0)
        {
            Debug.LogWarning("No placed objects found for drop effect");
            yield break;
        }
        //wait for the longest drop effect to finish
        float maxDuration = dropEffectDuration;
        yield return new WaitForSeconds(maxDuration + 0.1f);
        
        if (levelManager != null)
            levelManager.NextMiniGame();
    }
    public bool CheckAllConstraints()
    {
        if (miniLevelData.useBoundary)
        {
            foreach (var obj in objects)
            {
                if (obj == null || !obj.isPlaced) continue;
                if (!GridConstraintChecker.CheckBoundary(boardManager, obj, obj.currentGridPosition))
                    return false;
            }
        }
        if (miniLevelData.useNonOverlap)
        {
            foreach (var obj in objects)
            {
                if (obj == null || !obj.isPlaced) continue;
                if (!GridConstraintChecker.CheckNonOverlap(obj, obj.currentGridPosition, objects))
                    return false;
            }
        }
        if (miniLevelData.useExactSlots)
        {
            foreach (var obj in objects)
            {
                if (obj == null || !obj.isPlaced) continue;
                if (!GridConstraintChecker.CheckExactSlots(obj, obj.currentGridPosition, objects, miniLevelData.exactSlotAssignments))
                    return false;
            }
        }
        if (miniLevelData.useAllObjects)
        {
            if (!GridConstraintChecker.CheckUseAllObjects(objects))
                return false;
        }
        if (miniLevelData.useFullCoverage)
        {
            if (!GridConstraintChecker.CheckFullCoverage(boardManager, objects))
                return false;
        }
        return true;
    }
    public void OnCheckConstraintsButtonClicked()
    {
        if (puzzleCompleted)
        {
            Debug.Log("Puzzle already completed, no need to check constraints.");
            return;
        }

        Debug.Log("=== Checking All Constraints ===");

        if (!CheckAllConstraints())
        {
            Debug.LogError("❌ Constraints not satisfied. Keep trying!");
            return;
        }

        Debug.Log("✅ All gameplay constraints satisfied");

        // ✅ Check landing zones if enabled
        LevelCSPVerifier verifier = GetComponent<LevelCSPVerifier>();
        if (verifier != null && miniLevelData.useLandingZones)
        {
            Debug.Log("=== Checking Landing Zone Requirements ===");

            // ✅ IMPORTANT: Pass actual placed objects to verifier
            verifier.SetDragObjects(objects);  // ← Pass the real objects array

            if (!verifier.VerifyPlayerPlacement())
            {
                Debug.LogError("❌ Landing zone requirements NOT met. Resetting minigame...");
                ResetMiniGame();
                return;
            }
        }

        Debug.Log("✅ All constraints and landing zones satisfied! Puzzle completed.");
        puzzleCompleted = true;
        StartCoroutine(PlayAllDropEffects());
    }
    private void ResetMiniGame()
    {
        foreach (var obj in objects)
        {
            if (obj != null)
            {
                obj.ReturnToStart();
                obj.isPlaced = false;
            }
        }
        puzzleCompleted = false;
    }
}