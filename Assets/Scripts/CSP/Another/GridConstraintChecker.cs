using System.Collections.Generic;
using UnityEngine;

public static class GridConstraintChecker
{
    /// <summary>
    /// Check if object fits within board boundaries
    /// </summary>
    public static bool CheckBoundary(GridBoardManager boardManager, GridDragObject obj, Vector2Int gridPos)
    {
        if (obj == null || boardManager == null) return false;

        Vector2Int[] shape = obj.GetCurrentShape();
        Vector2Int[] occupied = ShapeUtils.TranslateCells(shape, gridPos.x, gridPos.y);

        foreach (var cell in occupied)
        {
            if (!boardManager.IsInsideBoard(cell))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Check non-overlap constraint (no objects can share cells on same layer)
    /// </summary>
    public static bool CheckNonOverlap(GridDragObject obj, Vector2Int gridPos, GridDragObject[] allObjects)
    {
        if (obj == null) return false;

        Vector2Int[] shape = obj.GetCurrentShape();
        Vector2Int[] occupied = ShapeUtils.TranslateCells(shape, gridPos.x, gridPos.y);

        foreach (var otherObj in allObjects)
        {
            if (otherObj == null || otherObj == obj || !otherObj.isPlaced)
                continue;

            Vector2Int[] otherShape = otherObj.GetCurrentShape();
            Vector2Int[] otherOccupied = ShapeUtils.TranslateCells(otherShape, otherObj.currentGridPosition.x, otherObj.currentGridPosition.y);

            foreach (var cell in occupied)
            {
                foreach (var otherCell in otherOccupied)
                {
                    if (cell == otherCell)
                        return false; // Overlap detected
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Check layer-aware overlap (objects can overlap only if on different layers)
    /// </summary>
    public static bool CheckLayerOverlap(GridDragObject obj, Vector2Int gridPos, GridDragObject[] allObjects)
    {
        if (obj == null) return false;

        Vector2Int[] shape = obj.GetCurrentShape();
        Vector2Int[] occupied = ShapeUtils.TranslateCells(shape, gridPos.x, gridPos.y);

        foreach (var otherObj in allObjects)
        {
            if (otherObj == null || otherObj == obj || !otherObj.isPlaced)
                continue;

            // If on different layers, overlap is allowed
            if (obj.currentLayer != otherObj.currentLayer)
                continue;

            // Same layer - check for overlapping cells
            Vector2Int[] otherShape = otherObj.GetCurrentShape();
            Vector2Int[] otherOccupied = ShapeUtils.TranslateCells(otherShape, otherObj.currentGridPosition.x, otherObj.currentGridPosition.y);

            foreach (var cell in occupied)
            {
                foreach (var otherCell in otherOccupied)
                {
                    if (cell == otherCell)
                        return false; // Same cell + same layer = FAIL
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Check exact slots constraint
    /// </summary>
    public static bool CheckExactSlots(GridDragObject obj, Vector2Int gridPos, GridDragObject[] allObjects, SlotAssignment[] exactSlotAssignments)
    {
        if (obj == null || exactSlotAssignments == null || exactSlotAssignments.Length == 0)
            return true;

        foreach (var assignment in exactSlotAssignments)
        {
            if (assignment.objectId == obj.objectId)
            {
                if (assignment.slotPosition == null || assignment.slotPosition.Length == 0)
                    return true;

                foreach (var slotPos in assignment.slotPosition)
                {
                    if (gridPos == slotPos)
                        return true;
                }
                return false; // Object must be at exact slot
            }
        }
        return true;
    }

    /// <summary>
    /// Check if all objects are placed
    /// </summary>
    public static bool CheckUseAllObjects(GridDragObject[] allObjects)
    {
        if (allObjects == null) return false;

        foreach (var obj in allObjects)
        {
            if (obj == null || !obj.isPlaced)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Check if all board cells are covered
    /// </summary>
    public static bool CheckFullCoverage(GridBoardManager boardManager, GridDragObject[] allObjects)
    {
        if (boardManager == null || allObjects == null) return false;

        HashSet<Vector2Int> coveredCells = new HashSet<Vector2Int>();

        foreach (var obj in allObjects)
        {
            if (obj == null || !obj.isPlaced)
                continue;

            Vector2Int[] shape = obj.GetCurrentShape();
            Vector2Int[] occupied = ShapeUtils.TranslateCells(shape, obj.currentGridPosition.x, obj.currentGridPosition.y);

            foreach (var cell in occupied)
            {
                coveredCells.Add(cell);
            }
        }

        int totalCells = boardManager.boardWidth * boardManager.boardHeight;
        return coveredCells.Count == totalCells;
    }
}