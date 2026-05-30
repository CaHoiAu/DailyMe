using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DressUpGameManager : BaseMiniGameManager
{
    [Header("References")]
    public EquipmentManager equipmentManager;
    public LevelManager levelManager;

    private DressUpMiniLevelData levelData;
    private List<GameObject> spawnedItems = new List<GameObject>();
    private GameObject spawnedBackground;
    private GameObject spawnedRackSystem;
    private GameObject spawnedCharacter;
    private Transform rackContainer;
    private RackSnapController rackSnapController;
    private bool isCompleted = false;

    public override void LoadMiniGame(ScriptableObject minigame)
    {
        levelData = minigame as DressUpMiniLevelData;
        if (levelData == null)
        {
            Debug.LogError("[DressUpGameManager] Invalid data — expected DressUpMiniLevelData");
            return;
        }
        isCompleted = false;
        ClearAll();
        SpawnBackground();
        SpawnCharacter();
        SpawnRackSystem();
        SpawnRackItems();
        equipmentManager?.SetInitialSprite(levelData.initialSprite);
        Debug.Log("[DressUpGameManager] Loaded: " + levelData.name);
    }

    public override void ResetMiniGame()
    {
        if (levelData == null) return;
        isCompleted = false;
        ClearAll();
        SpawnBackground();
        SpawnCharacter();
        SpawnRackSystem();
        SpawnRackItems();
        equipmentManager?.SetInitialSprite(levelData.initialSprite);
    }

    public override bool IsMiniGameCompleted()
    {
        if (levelData == null || equipmentManager == null) return false;
        string equippedId = equipmentManager.GetEquippedItemId();
        foreach (var entry in levelData.rackItems)
        {
            if (entry.isCorrect && entry.itemId == equippedId)
                return true;
        }
        return false;
    }

    public void OnCheckConstraintsButtonClicked()
    {
        if (isCompleted) return;
        if (!IsMiniGameCompleted())
        {
            Debug.Log("[DressUpGameManager] ❌ Chưa chọn đúng trang phục!");
            return;
        }
        Debug.Log("[DressUpGameManager] ✅ Trang phục đúng! Chuyển màn...");
        isCompleted = true;
        levelManager?.NextMiniGame();
    }
    private void SpawnCharacter()
    {
        if (levelData.characterPrefab == null) return;
        spawnedCharacter = Instantiate(levelData.characterPrefab, transform);
        spawnedCharacter.transform.position = levelData.characterPosition;

        // Wire EquipmentManager from spawned character if not manually assigned
        if (equipmentManager == null)
            equipmentManager = spawnedCharacter.GetComponentInChildren<EquipmentManager>();
    }

    private void SpawnBackground()
    {
        if (levelData.backgroundSprite == null) return;
        spawnedBackground = new GameObject("DressUp_Background");
        spawnedBackground.transform.SetParent(transform);
        spawnedBackground.transform.position = levelData.backgroundPosition;
        spawnedBackground.transform.localScale = levelData.backgroundScale;
        SpriteRenderer sr = spawnedBackground.AddComponent<SpriteRenderer>();
        sr.sprite = levelData.backgroundSprite;
        sr.sortingOrder = -1;
    }

    private void SpawnRackSystem()
    {
        if (levelData.rackSystemPrefab == null) return;
        spawnedRackSystem = Instantiate(levelData.rackSystemPrefab, transform);
        spawnedRackSystem.transform.position = levelData.rackPosition;

        // Find rack components from spawned prefab
        rackContainer = spawnedRackSystem.GetComponentInChildren<RackDragController>()?.transform.Find("RackContainer")
                        ?? spawnedRackSystem.transform;
        rackSnapController = spawnedRackSystem.GetComponentInChildren<RackSnapController>();

        // Wire EquipmentManager into snap controller
        if (rackSnapController != null)
            rackSnapController.equipmentManager = equipmentManager;
    }

    private void SpawnRackItems()
    {
        if (levelData.rackItems == null || rackContainer == null) return;
        for (int i = 0; i < levelData.rackItems.Length; i++)
        {
            ClothingRackEntry entry = levelData.rackItems[i];
            if (entry.prefab == null) continue;

            GameObject obj = Instantiate(entry.prefab, rackContainer);
            obj.transform.localPosition = new Vector3(i * levelData.itemSpacing, 0f, 0f);
            ClothingItemData data = obj.GetComponent<ClothingItemData>();
            if (data != null)
            {
                data.itemId = entry.itemId;
                data.clothingSprite = entry.clothingSprite;
            }
            spawnedItems.Add(obj);
        }
        rackSnapController?.SnapToNearestItem();
    }

    private void ClearAll()
    {
        foreach (var item in spawnedItems)
            if (item != null) Destroy(item);
        spawnedItems.Clear();

        if (spawnedRackSystem != null) { Destroy(spawnedRackSystem); spawnedRackSystem = null; }
        if (spawnedBackground != null) { Destroy(spawnedBackground); spawnedBackground = null; }
        if (spawnedCharacter != null) { Destroy(spawnedCharacter); spawnedCharacter = null; }

        rackContainer = null;
        rackSnapController = null;
        if (levelData?.characterPrefab != null) equipmentManager = null; // reset so next spawn re-wires
    }
}
