using System.Collections.Generic;
using UnityEngine;

public class CSPVariables
{
    public string objectId;
    public List<PlacementValue> domain = new List<PlacementValue>();

    public CSPVariables(string objectId)
    {
        this.objectId = objectId;
    }
}
