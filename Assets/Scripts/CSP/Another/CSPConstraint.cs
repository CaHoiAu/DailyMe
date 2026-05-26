using System.Collections.Generic;

public abstract class ICSPConstraint
{
    // danh sách biến mà constraint này liên quan
    public List<string> variables;

    public ICSPConstraint(List<string> variables)
    {
        this.variables = variables;
    }

    // kiểm tra constraint có thỏa mãn với assignment hiện tại không
    public abstract bool IsSatisfied(
        Dictionary<string, PlacementValue> assignment
    );
    public string GetErrorMessage()
    {
        return "Constraint violated: " + this.GetType().Name;
    }
}