using System.Collections.Generic;
using UnityEngine;

public class CSPAdjacentConstraint : ICSPConstraint
{
    private string objIdA, objIdB;
    private Dictionary<string, GridObjectState[]> objectStates;

    public CSPAdjacentConstraint(string objIdA, string objIdB, Dictionary<string, GridObjectState[]> objectStates) : base(new List<string> { objIdA, objIdB })
    {
        this.objIdA = objIdA;
        this.objIdB = objIdB;
        this.objectStates = objectStates;
    }

    public override bool IsSatisfied(Dictionary<string, PlacementValue> assignment)
    {
        string idA = variables[0];
        string idB = variables[1];

        if (!assignment.ContainsKey(idA) || !assignment.ContainsKey(idB))
        {
            return true; // Not enough information to evaluate the constraint
        }

        var cellsA = GetOccupiedCells(idA, assignment[idA]);
        var cellsB = new HashSet<Vector2Int>(GetOccupiedCells(idB, assignment[idB]));

        Vector2Int[] dirs = new Vector2Int[] {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(1, 1),   // UpRight
            new Vector2Int(1, -1),  // DownRight
            new Vector2Int(-1, 1),  // UpLeft
            new Vector2Int(-1, -1)  // DownLeft    
        };
        foreach ( var cell in cellsA )
        {
            foreach ( var dir in dirs )
            {
                if (cellsB.Contains(cell + dir))
                {
                    return true; // Found adjacent cells
                }
            }
        }
        return false;
    }

    private List<Vector2Int> GetOccupiedCells(string id, PlacementValue placement)
    {
        var result = new List<Vector2Int>();
        if (!objectStates.ContainsKey(id))
        {
            return result;
        }

        var state = objectStates[id];
        if (placement.stateIndex >= state.Length)
        {
            return result;
        }
        var rotated = ShapeUtils.RotateShape(state[placement.stateIndex].cells, placement.rotation);

        foreach (var cell in rotated)
        {
            result.Add(new Vector2Int(cell.x + placement.x, cell.y + placement.y));
        }
        return result;
    } 
}
