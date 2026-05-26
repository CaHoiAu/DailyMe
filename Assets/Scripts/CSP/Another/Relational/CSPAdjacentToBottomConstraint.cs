using System.Collections.Generic;
using UnityEngine;

public class CSPAdjacentToBottomConstraint : ICSPConstraint
{
    private string objectIdA, objectIdB;
    private Dictionary<string, GridObjectState[]> objectStates;

    public CSPAdjacentToBottomConstraint(
        string objectIdA, string objectIdB,
        Dictionary<string, GridObjectState[]> objectStates)
        : base(new List<string> { objectIdA, objectIdB })
    {
        this.objectIdA = objectIdA;
        this.objectIdB = objectIdB;
        this.objectStates = objectStates;
    }

    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
        if (!assignment.ContainsKey(objectIdA) || !assignment.ContainsKey(objectIdB))
            return true;

        var cellsA = GetCells(objectIdA, assignment[objectIdA]);
        var cellsB = new HashSet<Vector2Int>(GetCells(objectIdB, assignment[objectIdB]));

        // ✅ Tìm y thấp nhất của A
        int minY = int.MaxValue;
        foreach (var cell in cellsA)
            minY = Mathf.Min(minY, cell.y);

        // ✅ Lấy tất cả cell của A ở hàng thấp nhất
        var bottomCells = new List<Vector2Int>();
        foreach (var cell in cellsA)
            if (cell.y == minY)
                bottomCells.Add(cell);

        // ✅ B phải kề trái hoặc phải với 1 trong các bottom cells
        Vector2Int[] sideDirs = { Vector2Int.left, Vector2Int.right };
        foreach (var cell in bottomCells)
            foreach (var dir in sideDirs)
                if (cellsB.Contains(cell + dir))
                    return true;

        return false;
    }

    private List<Vector2Int> GetCells(string id, PlacementValue p)
    {
        var result = new List<Vector2Int>();
        if (!objectStates.ContainsKey(id)) return result;

        var states = objectStates[id];
        if (p.stateIndex >= states.Length) return result;

        var rotated = ShapeUtils.RotateShape(states[p.stateIndex].cells, p.rotation);
        foreach (var cell in rotated)
            result.Add(new Vector2Int(p.x + cell.x, p.y + cell.y));

        return result;
    }
}