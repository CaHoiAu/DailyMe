using UnityEngine;

[System.Serializable]
public class GridObjectData
{
    public string objectId;
    public GameObject prefab;
    public GridObjectState[] states;
    public Vector3 startWorldPosition;
    public int currentLayer = 0;
    public Vector3 scale = Vector3.one;

    public bool requiresPlacement = true; //whether this object needs to be placed on the grid

    [Header("Placement Restrictions")]
    public Vector2Int[] forbiddenCells = new Vector2Int[0]; //cells this object can never be placed on

    [Header("Drop Effect")]
    public float dropDuration = 2f; //how long the drop effect lasts
    public float dropHeight = 2f; //how far the object drops from its original position
    public AnimationCurve dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}
