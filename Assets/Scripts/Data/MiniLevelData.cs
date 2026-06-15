using UnityEngine;

[CreateAssetMenu(fileName = "New Mini Level", menuName = "Level/Mini Level")]
public class MiniLevelData : BaseMiniLevelData
{
    public MiniLevelType miniLevelType;
    public ObjectData[] objects;
    public SlotData[] slots;
    public ConstraintData[] constraints;
    public GameObject boardPrefab;
    public Vector3 boardPosition;

}
public enum MiniLevelType
{
    Arrange,
    DressUp,
    CutsceneOnly,
}
[System.Serializable]
public class ObjectData
{
    public string objectId;
    public string objectType;
    public GameObject prefab;
    public Vector3 startPosition;
    public int width;
    public int height;
}
[System.Serializable]
public class SlotData
{
    public string slotId;
    public int maxLayer;
    public GameObject prefab;
    public Vector3 position;
}
[System.Serializable]
public class ObjectVisualState
{
    public Sprite sprite;
    public bool isVertical;
    public int width;
    public int height;
}