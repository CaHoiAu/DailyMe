using System.Collections.Generic;

public class CSPSolver
{
    private List<CSPVariables> variables;
    private List<ICSPConstraint> constraints;
    private List<Dictionary<string, PlacementValue>> solutions = new List<Dictionary<string, PlacementValue>>();

    public CSPSolver(List<CSPVariables> variables, List<ICSPConstraint> constraints)
    {
        this.variables = variables;
        this.constraints = constraints;
    }

    public List<Dictionary<string, PlacementValue>> SolveUpTo(int maxSolutions)
    {
        solutions.Clear();

        int backtrackCount = 0;
        Backtrack(new Dictionary<string, PlacementValue>(), 0, maxSolutions, ref backtrackCount);

        return solutions;
    }

    private void Backtrack(Dictionary<string, PlacementValue> assignment, int index, int maxSolutions, ref int callCount)
    {
        callCount++;

        if (solutions.Count >= maxSolutions)
            return;

        // ✅ Base case: all variables assigned
        if (index == variables.Count)
        {

            // ✅ Only now check ALL constraints with complete assignment
            if (IsConsistent(assignment))
            {
                solutions.Add(new Dictionary<string, PlacementValue>(assignment));
            }
            return;
        }

        CSPVariables variable = variables[index];
        int validCount = 0;
        int tryCount = 0;

        foreach (PlacementValue value in variable.domain)
        {
            tryCount++;
            assignment[variable.objectId] = value;

            // ✅ Check only constraints that involve currently assigned variables
            if (IsConsistentPartial(assignment))
            {
                validCount++;
                Backtrack(assignment, index + 1, maxSolutions, ref callCount);
            }

            assignment.Remove(variable.objectId);

            if (solutions.Count >= maxSolutions)
                return;
        }
    }

    // ✅ Check constraints only for assigned variables during backtracking
    private bool IsConsistentPartial(Dictionary<string, PlacementValue> assignment)
    {
        foreach (ICSPConstraint constraint in constraints)
        {
            // ✅ Skip constraints that require unassigned variables
            foreach (var varId in constraint.variables)
            {
                if (!assignment.ContainsKey(varId))
                {
                    // Can't evaluate this constraint yet
                    continue; // Skip this constraint for now
                }
            }

            // ✅ Only evaluate if all required variables are assigned
            bool allVarsAssigned = true;
            foreach (var varId in constraint.variables)
            {
                if (!assignment.ContainsKey(varId))
                {
                    allVarsAssigned = false;
                    break;
                }
            }

            if (allVarsAssigned)
            {
                if (!constraint.IsSatisfied(assignment))
                    return false;
            }
        }

        return true;
    }

    // ✅ Final check: ALL constraints with complete assignment
    private bool IsConsistent(Dictionary<string, PlacementValue> assignment)
    {
        foreach (ICSPConstraint constraint in constraints)
        {
            if (!constraint.IsSatisfied(assignment))
            {
                return false;
            }
        }

        return true;
    }
}