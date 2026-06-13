using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ConstraintManager;

public class LevelCSPVerifier : MonoBehaviour
{
    [Header("Grid Mini-game References")]
    public GridMiniLevelData miniLevelData;
    public GridBoardManager boardManager;
    public GridDragObject[] dragObjects;

    //runtime sau mỗi lần player thẻ obj
    public bool VerifyCurrentPlacement()
    {
        var assignment = new Dictionary<string, PlacementValue>();
        var constraints = BuildGridConstraints(BuidGridVariables());

        var failed = new List<string>();
        foreach (var constraint in constraints)
        {
            if (!constraint.IsSatisfied(assignment))
            {
                failed.Add(constraint.GetType().Name);
            }
        }
        if(failed.Count == 0)
        {
            Debug.Log("✅ Current placement satisfies all constraints!");
        }
        foreach (var f in failed)
            Debug.LogError($"❌ Violated constraint: {f}");
        return false;
    }

    public Dictionary<string, PlacementValue> BuildAssignmentFromGameplay()
    {
        var assignment = new Dictionary<string, PlacementValue>();
        foreach (var obj in dragObjects)
        {
            if (obj == null || !obj.isPlaced) continue;
            assignment[obj.objectId] = new PlacementValue(
                obj.currentGridPosition.x,
                obj.currentGridPosition.y,
                obj.currentRotationIndex,
                obj.currentStateIndex,
                obj.GetLayer()
            );
        }
        return assignment;
    }

    [ContextMenu("Verify CSP Solution")]
    public void VerifyGridCSPSolution()
    {
        if (miniLevelData == null)
        {
            Debug.LogError("❌ Assign miniLevelData trước!");
            return;
        }

        // ✅ Luôn tạo mới từ miniLevelData — không cần assign thủ công
        dragObjects = CreateGridDragObjectsFromData(miniLevelData);

        if (dragObjects.Length == 0)
        {
            Debug.LogError("❌ Không có objects trong miniLevelData!");
            return;
        }
        try
        {
            Debug.Log("=== PRE-FLIGHT CHECKS ===");

            // Check 1: Shape validity
            foreach (var obj in dragObjects)
            {
                GridObjectState state = obj.GetCurrentState();
                if (state == null)
                {
                    Debug.LogError($"❌ {obj.objectId}: No current state!");
                    continue;
                }

                // Check for negative coordinates
                bool hasNegative = false;
                foreach (var cell in state.cells)
                {
                    if (cell.x < 0 || cell.y < 0)
                    {
                        hasNegative = true;
                        break;
                    }
                }
                Debug.Log($"{obj.objectId}: cells={state.cells.Length}, hasNegative={hasNegative}, allowRotate={state.allowRotate}");
            }

            // Check 2: Board dimensions
            Debug.Log($"Board: {miniLevelData.boardWidth}x{miniLevelData.boardHeight} = {miniLevelData.boardWidth * miniLevelData.boardHeight} cells");

            // Check 3: Total object cells
            int totalCells = 0;
            foreach (var obj in dragObjects)
            {
                GridObjectState state = obj.GetCurrentState();
                if (state != null) totalCells += state.cells.Length;
            }
            Debug.Log($"Total object cells: {totalCells}");

            // Check 4: Domain sizes
            List<CSPVariables> vars = BuidGridVariables();
            foreach (var v in vars)
            {
                Debug.Log($"{v.objectId}: domain size = {v.domain.Count}");
                if (v.domain.Count == 0)
                {
                    Debug.LogError($"❌ {v.objectId} HAS EMPTY DOMAIN!");
                }
            }

            if (miniLevelData == null || boardManager == null)
            {
                Debug.LogError("Please assign miniLevelData and boardManager in the inspector.");
                return;
            }

            if (dragObjects == null || dragObjects.Length == 0)
            {
                dragObjects = CreateGridDragObjectsFromData(miniLevelData);
                if (dragObjects.Length == 0)
                {
                    Debug.LogError("No GridDragObject found in scene. Please assign them in the inspector.");
                    return;
                }
                Debug.Log($"Auto-created {dragObjects.Length} GridDragObject(s) from GridMiniLevelData.");
            }

            // ✅ QUAN TRỌNG: Sync với game objects để lấy đúng current state
            SyncDragObjectsFromGameplay();

            // Debug: Print variables
            DebugPrintVariables();

            List<CSPVariables> variables = BuidGridVariables();
            List<ICSPConstraint> constraints = BuildGridConstraints(variables);

            // Debug: Print constraints
            DebugPrintConstraints(constraints);

            CSPSolver solver = new CSPSolver(variables, constraints);
            List<Dictionary<string, PlacementValue>> solutions = solver.SolveUpTo(7);

            Debug.Log("==== Grid Level Validation ====");
            Debug.Log($"Board Size: {miniLevelData.boardWidth}x{miniLevelData.boardHeight}");
            Debug.Log($"Number of objects: {dragObjects.Length}");
            Debug.Log($"Total domain size: {variables.Sum(v => v.domain.Count)}");
            Debug.Log($"Number of constraints: {constraints.Count}");
            Debug.Log($"Number of solutions found: {solutions.Count}");

            if (solutions.Count == 0)
            {
                Debug.LogError("❌ Level impossible - No valid solution found");
                DebugWhyNoSolution(variables, constraints);
            }
            else if (solutions.Count == 1)
            {
                Debug.Log("✅ Level valid: unique solution");
                PrintGridSolution(solutions[0]);
            }
            else
            {
                Debug.Log("⚠️ Level has multiple solutions - Consider adding more constraints to make it unique");
                for (int i = 0; i < solutions.Count; i++)
                {
                    Debug.Log($"--- Solution {i + 1} ---");
                    PrintGridSolution(solutions[i]);
                }
            }
        }
        finally
        {
            // ✅ Cleanup temp objects sau khi verify xong
            CleanupTempObjects();
        }
    }

    public bool VerifyPlayerPlacement()
    {
        Debug.Log("=== Verifying Player Placement ===");

        if (!miniLevelData.useLandingZones || miniLevelData.landingZones.Length == 0)
        {
            Debug.Log("✅ No landing zones required");
            return true;
        }

        // ✅ Step 1: Build map of objectId → required zone
        Dictionary<string, LandingZoneData> objectToZone = new Dictionary<string, LandingZoneData>();
        foreach (var zone in miniLevelData.landingZones)
        {
            if (zone.isVisualOnly) continue;
            if (zone.objectId != null && zone.objectId.Length > 0)
            {
                foreach (var objId in zone.objectId)
                {
                    if (!objectToZone.ContainsKey(objId))
                    {
                        objectToZone[objId] = zone;
                    }
                    else
                    {
                        Debug.LogError($"❌ Object '{objId}' assigned to multiple landing zones!");
                        return false;
                    }
                }
            }
        }

        // ✅ Step 2: Check each object with landing zone requirement
        foreach (var kvp in objectToZone)
        {
            string objectId = kvp.Key;
            LandingZoneData requiredZone = kvp.Value;

            GridDragObject obj = System.Array.Find(dragObjects, o => o != null && o.objectId == objectId);

            if (obj == null)
            {
                Debug.LogError($"❌ Object '{objectId}' required by zone '{requiredZone.zoneId}' not found!");
                return false;
            }

            if (!obj.isPlaced)
            {
                Debug.LogError($"❌ Object '{objectId}' not placed but zone '{requiredZone.zoneId}' requires it!");
                return false;
            }

            Vector2Int expectedGridPos = boardManager.WorldToGrid(requiredZone.position);
            Vector2Int actualGridPos = obj.currentGridPosition;

            // ✅ Check ONLY X position, ignore Y
            if (actualGridPos.x != expectedGridPos.x)
            {
                Debug.LogError($"❌ Object '{objectId}' at X={actualGridPos.x} but zone '{requiredZone.zoneId}' requires X={expectedGridPos.x}!");
                return false;
            }

            Debug.Log($"✅ Object '{objectId}' in correct zone '{requiredZone.zoneId}' at X={expectedGridPos.x} (Y can be anywhere)");
        }

        // ✅ Step 3: Check no wrong objects placed in landing zones (only check X)
        foreach (var zone in miniLevelData.landingZones)
        {
            if (zone.isVisualOnly) continue;
            Vector2Int zoneGridPos = boardManager.WorldToGrid(zone.position);
            int zoneX = zoneGridPos.x;

            // Check which objects are at this X position
            foreach (var obj in dragObjects)
            {
                if (obj == null || !obj.isPlaced) continue;

                if (!objectToZone.ContainsKey(obj.objectId)) continue; // no zone requirement — can go anywhere

                if (obj.currentGridPosition.x == zoneX)  // ✅ Only check X
                {
                    // ✅ Check if this object SHOULD be in this zone
                    bool isAllowed = false;
                    if (zone.objectId != null)
                    {
                        foreach (var allowedObjId in zone.objectId)
                        {
                            if (allowedObjId == obj.objectId)
                            {
                                isAllowed = true;
                                break;
                            }
                        }
                    }

                    if (!isAllowed)
                    {
                        Debug.LogError($"❌ Object '{obj.objectId}' at X={obj.currentGridPosition.x} but NOT ALLOWED in zone '{zone.zoneId}'!");
                        Debug.LogError($"   Zone expects: [{string.Join(", ", zone.objectId ?? new string[0])}]");
                        return false;
                    }
                }
            }
        }

        Debug.Log("✅ All landing zone requirements verified successfully (X position only)!");
        return true;
    }
    public void SetDragObjects(GridDragObject[] objects)
    {
        dragObjects = objects;
    }
    /// <summary>
    /// Debug method to understand why no solution was found
    /// </summary>
    private void DebugWhyNoSolution(List<CSPVariables> variables, List<ICSPConstraint> constraints)
    {
        Debug.LogWarning("=== Debugging: Why no solution? ===");

        // Test 1: Check if any single object can be placed
        Debug.LogWarning("Test 1: Can each object be placed individually?");
        foreach (var variable in variables)
        {
            bool canPlace = false;
            foreach (var value in variable.domain)
            {
                var testAssignment = new Dictionary<string, PlacementValue> { { variable.objectId, value } };
                bool valid = true;
                foreach (var constraint in constraints)
                {
                    if (!constraint.IsSatisfied(testAssignment))
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    canPlace = true;
                    break;
                }
            }
            Debug.LogWarning($"  {variable.objectId}: {(canPlace ? "✓ CAN be placed" : "✗ CANNOT be placed")}");
        }

        // Test 2: Check constraint violations
        Debug.LogWarning("Test 2: Which constraints might be too strict?");
        foreach (var constraint in constraints)
        {
            Debug.LogWarning($"  - {constraint.GetType().Name}");
        }

        // Test 3: Check board size vs objects
        Debug.LogWarning("Test 3: Board vs Objects:");
        int totalBoardCells = miniLevelData.boardWidth * miniLevelData.boardHeight;
        Debug.LogWarning($"  Board cells: {totalBoardCells}");

        int totalObjectCells = 0;
        foreach (var obj in dragObjects)
        {
            GridObjectState currentState = obj.GetCurrentState();
            if (currentState != null && currentState.cells != null)
            {
                int objCells = currentState.cells.Length;
                totalObjectCells += objCells;
                Debug.LogWarning($"  {obj.objectId}: {objCells} cells");
            }
        }
        Debug.LogWarning($"  Total object cells: {totalObjectCells}");
        Debug.LogWarning($"  {(totalObjectCells == totalBoardCells ? "✓ Perfect fit" : totalObjectCells < totalBoardCells ? "⚠️ Objects don't cover board" : "✗ Objects overflow board")}");

        // Test 4: Check if objects fit on board individually
        Debug.LogWarning("Test 4: Can each object fit on board (ignoring other constraints)?");
        foreach (var obj in dragObjects)
        {
            GridObjectState currentState = obj.GetCurrentState();
            if (currentState == null || currentState.cells == null || currentState.cells.Length == 0)
            {
                Debug.LogWarning($"  {obj.objectId}: ✗ No valid states");
                continue;
            }

            Vector2Int[] shape = currentState.cells;
            bool canFit = false;

            for (int x = 0; x < miniLevelData.boardWidth && !canFit; x++)
            {
                for (int y = 0; y < miniLevelData.boardHeight && !canFit; y++)
                {
                    Vector2Int[] occupied = ShapeUtils.TranslateCells(shape, x, y);
                    bool inside = true;
                    foreach (var cell in occupied)
                    {
                        if (cell.x < 0 || cell.x >= miniLevelData.boardWidth ||
                            cell.y < 0 || cell.y >= miniLevelData.boardHeight)
                        {
                            inside = false;
                            break;
                        }
                    }
                    if (inside) canFit = true;
                }
            }
            Debug.LogWarning($"  {obj.objectId}: {(canFit ? "✓ CAN fit" : "✗ CANNOT fit")}");
        }
    }

    private void DebugPrintVariables()
    {
        Debug.Log("=== Variables ===");
        foreach (var obj in dragObjects)
        {
            GridObjectState currentState = obj.GetCurrentState();
            Debug.Log($"{obj.objectId}: currentStateIndex={obj.currentStateIndex}, currentRotation={obj.currentRotationIndex}");
            if (currentState != null)
            {
                Debug.Log($"  Current state ({currentState.stateName}): {currentState.cells.Length} cells, allowRotate={currentState.allowRotate}");
            }
        }
    }

    private void DebugPrintConstraints(List<ICSPConstraint> constraints)
    {
        Debug.Log("=== Constraints ===");
        foreach (var constraint in constraints)
        {
            Debug.Log($"  - {constraint.GetType().Name}");
        }
    }

    // Thay thế hoàn toàn CreateGridDragObjectsFromData
    private GridDragObject[] CreateGridDragObjectsFromData(GridMiniLevelData data)
    {
        if (data == null || data.objects == null || data.objects.Length == 0)
        {
            Debug.LogError("❌ miniLevelData không có objects!");
            return new GridDragObject[0];
        }

        var result = new List<GridDragObject>();

        foreach (var objData in data.objects)
        {
            if (objData == null) continue;

            // ✅ Tạo GameObject tạm, không spawn vào scene thật
            GameObject tempObj = new GameObject($"[CSP_Temp] {objData.objectId}");
            tempObj.hideFlags = HideFlags.HideAndDontSave; // ẩn khỏi hierarchy

            // Thêm SpriteRenderer và Collider2D vì GridDragObject RequireComponent
            tempObj.AddComponent<SpriteRenderer>();
            tempObj.AddComponent<BoxCollider2D>();

            GridDragObject drag = tempObj.AddComponent<GridDragObject>();
            drag.objectId = objData.objectId;

            // ✅ Copy toàn bộ states từ data
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
            }

            drag.currentStateIndex = 0;
            drag.currentRotationIndex = 0;

            result.Add(drag);
        }

        Debug.Log($"✅ Created {result.Count} virtual objects from miniLevelData");
        return result.ToArray();
    }

    // ✅ Cleanup temp objects sau khi verify xong
    private void CleanupTempObjects()
    {
        if (dragObjects == null) return;
        foreach (var obj in dragObjects)
        {
            if (obj != null && obj.gameObject.name.StartsWith("[CSP_Temp]"))
                DestroyImmediate(obj.gameObject);
        }
        dragObjects = null;
    }
    private List<CSPVariables> BuidGridVariables()
    {
        List<CSPVariables> variables = new List<CSPVariables>();
        foreach (GridDragObject obj in dragObjects)
        {
            if (obj == null || obj.states == null || obj.states.Length == 0)
                continue;
            var objData = GetObjectData(obj.objectId);
            if (objData != null && !objData.requiresPlacement)
                continue;

            var variable = new CSPVariables(obj.objectId);

            for (int stateIndex = 0; stateIndex < obj.states.Length; stateIndex++)
            {
                GridObjectState state = obj.states[stateIndex];
                if (state == null || state.cells == null || state.cells.Length == 0)
                    continue;
                int rotationCount = state.allowRotate ? 4 : 1;
                for (int x = 0; x < miniLevelData.boardWidth; x++)
                {
                    for (int y = 0; y < miniLevelData.boardHeight; y++)
                    {
                        for (int rot = 0; rot < rotationCount; rot++)
                        {
                            var rotatedShape = ShapeUtils.RotateShape(state.cells, rot);
                            var translatedShape = ShapeUtils.TranslateCells(rotatedShape, x, y);

                            bool fits = translatedShape.All(cell =>
                                cell.x >= 0 && cell.x < miniLevelData.boardWidth &&
                                cell.y >= 0 && cell.y < miniLevelData.boardHeight);
                            if (fits)
                            {
                                var layer = obj.GetComponent<GridDragObject>()?.GetLayer() ?? 0;
                                variable.domain.Add(new PlacementValue(x, y, rot, stateIndex, layer));
                            }
                        }
                    }
                }
            }
            variables.Add(variable);
        }
        return variables;
    }

    private GridObjectData GetObjectData(string objectId)
    {
        if (miniLevelData?.objects == null) return null;
        foreach (var obj in miniLevelData.objects)
            if (obj.objectId == objectId) return obj;
        return null;
    }

    private List<ICSPConstraint> BuildGridConstraints(List<CSPVariables> variables)
    {
        List<ICSPConstraint> constraints = new List<ICSPConstraint>();
        var allObjectIds = variables.Select(v => v.objectId).ToList();

        var allStatesDict = new Dictionary<string, GridObjectState[]>();
        foreach (GridDragObject obj in dragObjects)
        {
            if (obj != null && obj.states != null)
            {
                allStatesDict[obj.objectId] = obj.states;
            }
        }

        // ✅ Boundary constraints
        foreach (GridDragObject obj in dragObjects)
        {
            if (obj == null || obj.states == null || obj.states.Length == 0)
                continue;
            constraints.Add(new CSPBoundaryContraint(
                obj.objectId, obj.states, miniLevelData.boardWidth, miniLevelData.boardHeight));
        }

        // ✅ Forbidden cells constraint
        foreach (GridDragObject obj in dragObjects)
        {
            if (obj == null || obj.states == null || obj.states.Length == 0)
                continue;
            if (obj.forbiddenCells == null || obj.forbiddenCells.Length == 0)
                continue;
            constraints.Add(new CSPForbiddenCellsConstraint(
                obj.objectId, obj.states, obj.forbiddenCells));
        }

        // ✅ Non-overlap constraint
        if (miniLevelData.useNonOverlap)
        {
            constraints.Add(new CSPNonOverlapConstraint(allStatesDict));
        }

        // ✅ Use all objects constraint
        if (miniLevelData.useAllObjects)
        {
            var useAllConstraint = new CSPUseAllObjectsConstraint(allObjectIds);
            constraints.Add(useAllConstraint);
        }

        // ✅ Full coverage constraint
        if (miniLevelData.useFullCoverage)
        {
            constraints.Add(new CSPFullCoverageConstraint(allStatesDict,miniLevelData.boardWidth, miniLevelData.boardHeight));
        }

        // ✅ Exact slots constraint
        if (miniLevelData.useExactSlots && miniLevelData.exactSlotAssignments.Length > 0)
        {
            foreach (var slotAssignment in miniLevelData.exactSlotAssignments)
            {
                var exactSlotConstraint = new CSPExactSlotConstraint(
                    slotAssignment.objectId,
                    slotAssignment.slotPosition
                );
                constraints.Add(exactSlotConstraint);
            }
        }

        //✅ adjacent constraint
        if (miniLevelData.adjacentConstraints != null)
        {
            foreach (var pair in miniLevelData.adjacentConstraints)
            {
                var adjacentConstraint = new CSPAdjacentConstraint(pair.objIdA, pair.objIdB, allStatesDict);
                constraints.Add(adjacentConstraint);
            }
        }

        //✅ require state constraint
        if (miniLevelData.requireStateConstraints != null)
        {
            foreach (var req in miniLevelData.requireStateConstraints)
            {
                var requireStateConstraint = new CSPRequireState(req.objectId, req.requiredStateIndex);
                constraints.Add(requireStateConstraint);
            }
        }

        //✅ relative postion constraint
        if (miniLevelData.relativePositionConstraints != null)
        {
            foreach (var rel in miniLevelData.relativePositionConstraints)
            {
                var relativePositionConstraint = new CSPRelativeDir(rel.objectIdA, rel.objectIdB, rel.direction);
                constraints.Add(relativePositionConstraint);
            }
        }

        //✅ below adjacent constraint
        if (miniLevelData.belowAdjacentConstraints != null)
            foreach (var c in miniLevelData.belowAdjacentConstraints)
                constraints.Add(new CSPAdjacentToBottomConstraint(
                    c.objectIdA, c.objectIdB, allStatesDict));

        //✅ require state only constraint
        foreach (var objData in miniLevelData.objects)
        {
            if (objData.requiresPlacement) { continue; }

            if (miniLevelData.requireStateConstraints == null) { continue; }
            foreach (var c in miniLevelData.requireStateConstraints)
            {
                if (c.objectId == objData.objectId)
                {
                    var requireStateOnlyConstraint = new CSPRequireStateOnlyConstraint(
                        c.objectId, c.requiredStateIndex, dragObjects);
                    constraints.Add(requireStateOnlyConstraint);
                }
            }
        }
        return constraints;
    }

    private void PrintGridSolution(Dictionary<string, PlacementValue> solution)
    {
        foreach (var pair in solution)
        {
            Debug.Log($"{pair.Key} -> Pos: ({pair.Value.x}, {pair.Value.y}), Rot: {pair.Value.rotation}, State: {pair.Value.stateIndex}");
        }
    }

    private void SyncDragObjectsFromGameplay()
    {
        GridPuzzleManager manager = FindObjectOfType<GridPuzzleManager>();
        if (manager != null)
        {
            GridDragObject[] gameplayObjects = manager.GetComponentsInChildren<GridDragObject>();
            if (gameplayObjects != null && gameplayObjects.Length > 0)
            {
                // ✅ Sync TẤT CẢ objects, không chỉ [0]
                foreach (var gameplayObj in gameplayObjects)
                {
                    GridDragObject cspObj = System.Array.Find(dragObjects, o => o != null && o.objectId == gameplayObj.objectId);
                    if (cspObj != null)
                    {
                        cspObj.currentStateIndex = gameplayObj.currentStateIndex;
                        cspObj.currentRotationIndex = gameplayObj.currentRotationIndex;
                        Debug.Log($"✅ Synchronized '{cspObj.objectId}' state={cspObj.currentStateIndex}, rot={cspObj.currentRotationIndex}");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Object '{gameplayObj.objectId}' from gameplay not found in CSP verifier!");
                    }
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No gameplay objects found!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ GridPuzzleManager not found!");
        }
    }
}
