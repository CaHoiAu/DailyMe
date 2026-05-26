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

    [Header("CSP")]
    public ConstraintManager constraintManager;
    public LevelCSPVerifier levelCSPVerifier;

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
        SpawnBackground(data);
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
        SetupCSP();
        Debug.Log("=== PUZZLE LOADED COMPLETE ===");
    }
    private void SetupCSP()
    {
        if (levelCSPVerifier == null) return;

        levelCSPVerifier.miniLevelData = miniLevelData;
        levelCSPVerifier.boardManager = boardManager;
        levelCSPVerifier.SetDragObjects(objects);
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
        var stateDicts = new Dictionary<string, GridObjectState[]>();
        foreach (var obj in objects){
            if (obj != null && obj.states != null)
                stateDicts[obj.objectId] = obj.states;
        }
        if (stateDicts.Count > 0)
        {
            var layerConstraint = new CSPGridLayerOverlapConstraint(stateDicts);
            constraintManager.constraints.RemoveAll(c => c is CSPGridLayerOverlapConstraint);
            constraintManager.constraints.Add(layerConstraint);
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
    private void SpawnBackground(GridMiniLevelData data)
    {
        // ✅ Check if background sprite should be spawned
        if (data.backgroundSprite == null)
        {
            Debug.Log("Background sprite is not set");
            return;
        }

        // ✅ Create background GameObject
        GameObject background = new GameObject("Background");
        background.transform.SetParent(transform);
        background.transform.position = data.backgroundPosition;
        background.transform.localScale = data.backgroundScale;
        background.transform.localRotation = Quaternion.identity;

        // ✅ Add SpriteRenderer component
        SpriteRenderer spriteRenderer = background.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = data.backgroundSprite;
        spriteRenderer.color = data.backgroundColor;
        spriteRenderer.sortingOrder = -1; // ✅ Background should be behind everything

        Debug.Log($"Background sprite spawned at position: {data.backgroundPosition} with scale: {data.backgroundScale}");
    }
    private void SpawnLandingZones(GridMiniLevelData data)
    {
        // ✅ Check if landing zones should be spawned
        if (!data.useLandingZones || data.landingZones == null || data.landingZones.Length == 0)
        {
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
            obj.transform.localScale = objData.scale;

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
                        stateId = objData.states[i].stateId,
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

        // ✅ CHECK LAYER OVERLAP TRƯỚC để tránh CheckNonOverlap block layers khác
        if (miniLevelData.useLayerOverlap)
        {
            bool layerOverlap = GridConstraintChecker.CheckLayerOverlap(obj, gridPos, objects);
            if (!layerOverlap)
            {
                Debug.Log("Layer overlap constraint failed!");
                return false;
            }
        }
        // ✅ Nếu useLayerOverlap = false, thì dùng CheckNonOverlap
        else if (miniLevelData.useNonOverlap)
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

        // 🔍 DEBUG: In ra các cells bị chiếm
        Vector2Int[] occupiedCells = obj.GetOccupiedCellsAt(gridPos);
        string cellsDebug = string.Join(", ", System.Array.ConvertAll(occupiedCells, c => $"({c.x},{c.y})"));
        Debug.Log($"[{obj.objectId}] ✅ Placing at grid position {gridPos}");
        Debug.Log($"[{obj.objectId}] 📍 Occupied cells ({occupiedCells.Length}): {cellsDebug}");

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
        //if (puzzleCompleted) return;

        //if (CheckAllConstraints())
        //{
        //    if (miniLevelData.useLandingZones && levelCSPVerifier != null)
        //    {
        //        levelCSPVerifier = GetComponent<LevelCSPVerifier>();
        //        if (!levelCSPVerifier.VerifyPlayerPlacement()) 
        //        {
        //            return;
        //        }
        //    }
        //    Debug.Log("✅ Puzzle Complete!");
        //    puzzleCompleted = true;
        //    StartCoroutine(PlayAllDropEffects());
        //}
    }
    private Vector3 GetDropTargetPosition(GridDragObject obj)
    {
        // Get the grid position where the object was placed
        Vector3 gridWorldPos = boardManager.GridToWorld(obj.currentGridPosition);

        // ✅ GridToWorld đã trả về tâm cell, không cần cộng shape center thêm
        Vector3 gridCenterPos = gridWorldPos;

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
            if (levelManager != null)
                levelManager.NextMiniGame();
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
        Debug.Log("=== CheckAllConstraints ===");

        // ── Spatial Constraints ──────────────────────────────────────
        if (miniLevelData.useBoundary)
        {
            foreach (var obj in objects)
            {
                if (obj == null || !obj.isPlaced) continue;
                bool pass = GridConstraintChecker.CheckBoundary(boardManager, obj, obj.currentGridPosition);
                Debug.Log($"Boundary [{obj.objectId}]: {(pass ? "✅" : "❌")}");
                if (!pass) return false;
            }
        }

        if (miniLevelData.useNonOverlap)
        {
            foreach (var obj in objects)
            {
                if (obj == null || !obj.isPlaced) continue;
                bool pass = GridConstraintChecker.CheckNonOverlap(obj, obj.currentGridPosition, objects);
                Debug.Log($"NonOverlap [{obj.objectId}]: {(pass ? "✅" : "❌")}");
                if (!pass) return false;
            }
        }

        if (miniLevelData.useAllObjects)
        {
            bool pass = GridConstraintChecker.CheckUseAllObjects(objects);
            Debug.Log($"UseAllObjects: {(pass ? "✅" : "❌")}");
            if (!pass) return false;
        }

        if (miniLevelData.useFullCoverage)
        {
            bool pass = GridConstraintChecker.CheckFullCoverage(boardManager, objects);
            Debug.Log($"FullCoverage: {(pass ? "✅" : "❌")}");
            if (!pass) return false;
        }

        if (miniLevelData.useExactSlots)
        {
            foreach (var obj in objects)
            {
                if (obj == null || !obj.isPlaced) continue;
                bool pass = GridConstraintChecker.CheckExactSlots(obj, obj.currentGridPosition, objects, miniLevelData.exactSlotAssignments);
                Debug.Log($"ExactSlot [{obj.objectId}]: {(pass ? "✅" : "❌")}");
                if (!pass) return false;
            }
        }

        if (miniLevelData.useLayerOverlap)
        {
            foreach (var obj in objects)
            {
                if (obj == null || !obj.isPlaced) continue;
                bool pass = GridConstraintChecker.CheckLayerOverlap(obj, obj.currentGridPosition, objects);
                Debug.Log($"LayerOverlap [{obj.objectId}]: {(pass ? "✅" : "❌")}");
                if (!pass) return false;
            }
        }

        // ── Relational Constraints ───────────────────────────────────
        var assignment = constraintManager.BuildAssignement(objects);
        var statesDict = BuildStatesDict();

        if (miniLevelData.requireStateConstraints != null)
        {
            foreach (var c in miniLevelData.requireStateConstraints)
            {
                var constraint = new CSPRequireState(c.objectId, c.requiredStateIndex);
                bool pass = constraint.IsSatisfied(assignment);
                Debug.Log($"RequireState [{c.objectId}] state={c.requiredStateIndex}: {(pass ? "✅" : "❌")}");
                if (!pass) return false;
            }
        }

        if (miniLevelData.adjacentConstraints != null)
        {
            foreach (var c in miniLevelData.adjacentConstraints)
            {
                var constraint = new CSPAdjacentConstraint(c.objIdA, c.objIdB, statesDict);
                bool pass = constraint.IsSatisfied(assignment);
                Debug.Log($"Adjacent [{c.objIdA}] & [{c.objIdB}]: {(pass ? "✅" : "❌")}");
                if (!pass) return false;
            }
        }

        if (miniLevelData.relativePositionConstraints != null)
        {
            foreach (var c in miniLevelData.relativePositionConstraints)
            {
                var constraint = new CSPRelativeDir(c.objectIdA, c.objectIdB, c.direction);
                bool pass = constraint.IsSatisfied(assignment);
                Debug.Log($"RelativePos [{c.objectIdA}] {c.direction} [{c.objectIdB}]: {(pass ? "✅" : "❌")}");
                if (!pass) return false;
            }
        }

        if (miniLevelData.belowAdjacentConstraints != null)
        {
            foreach (var c in miniLevelData.belowAdjacentConstraints)
            {
                var constraint = new CSPAdjacentToBottomConstraint(c.objectIdA, c.objectIdB, statesDict);
                bool pass = constraint.IsSatisfied(assignment);
                Debug.Log($"BelowAdjacent [{c.objectIdA}] & [{c.objectIdB}]: {(pass ? "✅" : "❌")}");
                if (!pass) return false;
            }
        }

        // ── State Only Constraints ───────────────────────────────────
        foreach (var objData in miniLevelData.objects)
        {
            if (objData.requiresPlacement) continue;
            if (miniLevelData.requireStateConstraints == null) continue;

            foreach (var c in miniLevelData.requireStateConstraints)
            {
                if (c.objectId != objData.objectId) continue;
                var obj = System.Array.Find(objects, o => o.objectId == c.objectId);
                if (obj == null) continue;

                bool pass = obj.currentStateIndex == c.requiredStateIndex;
                Debug.Log($"StateOnly [{c.objectId}] state={c.requiredStateIndex}: {(pass ? "✅" : "❌")}");
                if (!pass) return false;
            }
        }

        Debug.Log("✅ All constraints satisfied!");
        return true;
    }
    private Dictionary<string, GridObjectState[]> BuildStatesDict()
    {
        var dict = new Dictionary<string, GridObjectState[]>();
        foreach (var obj in objects)
            if (obj != null && obj.states != null)
                dict[obj.objectId] = obj.states;
        return dict;
    }
    public void OnCheckConstraintsButtonClicked()
    {
        if (puzzleCompleted) return;

        Debug.Log("=== Checking All Constraints ===");

        if (!CheckAllConstraints())
        {
            Debug.Log("❌ Chưa thỏa mãn các constraint. Thử lại!");
            return;
        }

        // ✅ Check landing zones nếu có
        if (miniLevelData.useLandingZones && levelCSPVerifier != null)
        {
            levelCSPVerifier.SetDragObjects(objects);
            if (!levelCSPVerifier.VerifyPlayerPlacement())
            {
                Debug.Log("❌ Landing zones chưa đúng!");
                return;
            }
        }

        Debug.Log("✅ All constraints satisfied! Puzzle Complete!");
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