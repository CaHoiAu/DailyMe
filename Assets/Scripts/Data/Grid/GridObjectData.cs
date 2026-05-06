using UnityEngine;

[System.Serializable]
public class GridObjectData
{
    public string objectId;
    public GameObject prefab;
    public GridObjectState[] states;
    public Vector3 startWorldPosition;
    public int currentLayer = 0;

    [Header("Drop Effect")]
    public float dropDuration = 2f; //how long the drop effect lasts
    public float dropHeight = 2f; //how far the object drops from its original position
    public AnimationCurve dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}
