using UnityEngine;

[System.Serializable]
public class ClothingRackEntry
{
    public string itemId;
    public Sprite clothingSprite;
    public bool isCorrect;
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "NewDressUpLevel", menuName = "Game/DressUpMiniLevelData")]
public class DressUpMiniLevelData : BaseMiniLevelData
{
    [Header("Background")]
    public Sprite backgroundSprite;
    public Vector3 backgroundPosition;
    public Vector3 backgroundScale = Vector3.one;

    [Header("Rack System")]
    public GameObject rackSystemPrefab; // contains RackDragController, rackContainer, centerMark
    public Vector3 rackPosition;
    public float itemSpacing = 1.5f;

    [Header("Rack Items")]
    public ClothingRackEntry[] rackItems;

    [Header("Character")]
    public GameObject characterPrefab;
    public Vector3 characterPosition;
    public Sprite initialSprite;
}
