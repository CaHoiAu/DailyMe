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

    [ContextMenu("Verify CSP Solution")]
    public void VerifyGridCSPSolution()
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
        List<Dictionary<string, PlacementValue>> solutions = solver.SolveUpTo(2);

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
            Vector2Int zoneGridPos = boardManager.WorldToGrid(zone.position);
            int zoneX = zoneGridPos.x;

            // Check which objects are at this X position
            foreach (var obj in dragObjects)
            {
                if (obj == null || !obj.isPlaced) continue;

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

    private GridDragObject[] CreateGridDragObjectsFromData(GridMiniLevelData data)
    {
        if (data.objects == null || data.objects.Length == 0)
        {
            Debug.LogWarning("GridMiniLevelData has no objects defined.");
            return new GridDragObject[0];
        }

        List<GridDragObject> objects = new List<GridDragObject>();

        foreach (var objData in data.objects)
        {
            if (objData.prefab == null)
            {
                Debug.LogWarning($"GridObjectData '{objData.objectId}' has no prefab assigned.");
                continue;
            }

            GameObject objInstance = Instantiate(objData.prefab);
            GridDragObject dragObj = objInstance.GetComponent<GridDragObject>();

            if (dragObj == null)
            {
                Debug.LogWarning($"Prefab '{objData.prefab.name}' does not have GridDragObject component.");
                Destroy(objInstance);
                continue;
            }

            dragObj.objectId = objData.objectId;

            if (objData.states != null && objData.states.Length > 0)
            {
                dragObj.states = new GridObjectState[objData.states.Length];
                for (int i = 0; i < objData.states.Length; i++)
                {
                    dragObj.states[i] = new GridObjectState
                    {
                        stateName = objData.states[i].stateName,
                        sprite = objData.states[i].sprite,
                        cells = objData.states[i].cells,
                        allowRotate = objData.states[i].allowRotate
                    };
                }
            }

            dragObj.currentStateIndex = 0;
            dragObj.currentRotationIndex = 0;

            objects.Add(dragObj);
        }

        return objects.ToArray();
    }

    private List<CSPVariables> BuidGridVariables()
    {
        List<CSPVariables> variables = new List<CSPVariables>();
        foreach (GridDragObject obj in dragObjects)
        {
            if (obj == null || obj.states == null || obj.states.Length == 0)
                continue;

            // ✅ CHECK FIRST before using
            GridObjectState currentState = obj.GetCurrentState();
            if (currentState == null || currentState.cells == null || currentState.cells.Length == 0)
                continue;

            CSPVariables variable = new CSPVariables(obj.objectId);

            Vector2Int[] baseShape = currentState.cells;
            int maxRotation = currentState.allowRotate ? 4 : 1;

            // ✅ Generate all valid placements
            for (int x = 0; x < miniLevelData.boardWidth; x++)
            {
                for (int y = 0; y < miniLevelData.boardHeight; y++)
                {
                    for (int r = 0; r < maxRotation; r++)
                    {
                        // ✅ Rotate THEN translate
                        Vector2Int[] rotatedShape = ShapeUtils.RotateShape(baseShape, r);
                        Vector2Int[] translatedShape = ShapeUtils.TranslateCells(rotatedShape, x, y);

                        // ✅ Check if all cells are within board bounds
                        bool fits = true;
                        foreach (var cell in translatedShape)
                        {
                            if (cell.x < 0 || cell.x >= miniLevelData.boardWidth ||
                                cell.y < 0 || cell.y >= miniLevelData.boardHeight)
                            {
                                fits = false;
                                break;
                            }
                        }
                        int layer = obj.GetComponent<ILayerProvider>()?.GetLayer() ?? 0;

                        if (fits)
                        {
                            variable.domain.Add(new PlacementValue(x, y, r, obj.currentStateIndex, layer));
                        }
                    }
                }
            }

            if (variable.domain.Count == 0)
            {
                Debug.LogWarning($"⚠️ Object '{obj.objectId}' has NO valid placements! Shape might be too large or invalid.");
            }

            variables.Add(variable);
        }
        return variables;
    }

    private List<ICSPConstraint> BuildGridConstraints(List<CSPVariables> variables)
    {
        List<ICSPConstraint> constraints = new List<ICSPConstraint>();

        var allObjectIds = variables.Select(v => v.objectId).ToList();

        // ✅ Boundary constraints
        foreach (GridDragObject obj in dragObjects)
        {
            if (obj == null || obj.states == null || obj.states.Length == 0)
                continue;

            GridObjectState currentState = obj.GetCurrentState();
            if (currentState != null && currentState.cells != null && currentState.cells.Length > 0)
            {
                var boundaryConstraint = new CSPBoundaryContraint(
                    obj.objectId,
                    currentState.cells,
                    miniLevelData.boardWidth,
                    miniLevelData.boardHeight
                );
                constraints.Add(boundaryConstraint);
            }
        }

        // ✅ Non-overlap constraint
        if (miniLevelData.useNonOverlap)
        {
            var shapesDict = new Dictionary<string, Vector2Int[]>();
            foreach (GridDragObject obj in dragObjects)
            {
                if (obj == null) continue;
                GridObjectState currentState = obj.GetCurrentState();
                if (currentState != null && currentState.cells != null)
                {
                    shapesDict[obj.objectId] = currentState.cells;
                }
            }
            if (shapesDict.Count > 0)
            {
                var nonOverlapConstraint = new CSPNonOverlapConstraint(shapesDict);
                constraints.Add(nonOverlapConstraint);
            }
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
            var shapesDict = new Dictionary<string, Vector2Int[]>();
            foreach (GridDragObject obj in dragObjects)
            {
                if (obj == null) continue;
                GridObjectState currentState = obj.GetCurrentState();
                if (currentState != null && currentState.cells != null)
                {
                    shapesDict[obj.objectId] = currentState.cells;
                }
            }
            if (shapesDict.Count > 0)
            {
                var fullCoverageConstraint = new CSPFullCoverageConstraint(
                    shapesDict,
                    miniLevelData.boardWidth,
                    miniLevelData.boardHeight
                );
                constraints.Add(fullCoverageConstraint);
            }
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
