using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using static Unity.VisualScripting.Member;

public class DressUpGameManager : BaseMiniGameManager
{
    [Header("References")]
    public EquipmentManager equipmentManager;
    public LevelManager levelManager;
    [Tooltip("Fixed point in the scene marking the center display slot — must NOT be inside RackSystem")]
    public Transform centerMarker;

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
        ResolveAndWireEquipmentManager();
        rackSnapController?.SnapToNearestItem(equip: false);

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
        ResolveAndWireEquipmentManager();
        rackSnapController?.SnapToNearestItem(equip: false);

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
    // Called after all spawning — resolves EM from any source then wires every snap controller
    private void ResolveAndWireEquipmentManager()
    {
        if (equipmentManager == null)
            equipmentManager = FindObjectOfType<EquipmentManager>();

        if (equipmentManager == null)
        {
            Debug.LogError("[DressUpGameManager] EquipmentManager not found anywhere in scene!");
            return;
        }

        foreach (var snap in FindObjectsByType<RackSnapController>(FindObjectsSortMode.None))
        {
            snap.equipmentManager = equipmentManager;
            if (centerMarker != null)
                snap.centerMarker = centerMarker;
            if (snap.rackMoveTarget == null)
            {
                RackDragController drag = snap.GetComponent<RackDragController>()
                                       ?? snap.GetComponentInParent<RackDragController>()
                                       ?? snap.GetComponentInChildren<RackDragController>();
                if (drag != null) snap.rackMoveTarget = drag.transform;
            }
        }
        Debug.Log("[DressUpGameManager] EquipmentManager wired to all RackSnapControllers ✓");
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
        RackDragController dragController = spawnedRackSystem.GetComponentInChildren<RackDragController>();
        rackContainer = dragController?.transform.Find("RackContainer") ?? spawnedRackSystem.transform;
        rackSnapController = spawnedRackSystem.GetComponentInChildren<RackSnapController>();

        // Wire EquipmentManager into snap controller
        if (rackSnapController == null)
            Debug.LogError("[DressUpGameManager] RackSnapController not found in rack prefab!");
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
