using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public SpriteRenderer clothingRenderer;

    private string equippedItemId;
    void Awake()
    {
        if (clothingRenderer == null)
            clothingRenderer = GetComponent<SpriteRenderer>()
                            ?? GetComponentInChildren<SpriteRenderer>(true)
                            ?? GetComponentInParent<SpriteRenderer>();

        if (clothingRenderer == null)
            Debug.LogError("[EquipmentManager] clothingRenderer not found — assign BaseBody's SpriteRenderer manually in the Inspector.");
        else
            Debug.Log("[EquipmentManager] clothingRenderer auto-found: " + clothingRenderer.gameObject.name);
    }
    public void Equip(ClothingItemData item)
    {
        if (item == null) return;
        if (item.clothingSprite == null)
        {
            Debug.LogWarning("[EquipmentManager] clothingSprite is null on item: " + item.itemId);
            return;
        }
        if (clothingRenderer == null)
        {
            Debug.LogError("[EquipmentManager] clothingRenderer is not assigned!");
            return;
        }
        clothingRenderer.sprite = item.clothingSprite;
        equippedItemId = item.itemId;
        Debug.Log("[EquipmentManager] Equipped: " + item.itemId);
    }

    public void SetInitialSprite(Sprite sprite)
    {
        if (clothingRenderer != null) clothingRenderer.sprite = sprite;
        equippedItemId = null;
    }

    public void ClearOutfit()
    {
        if (clothingRenderer != null) clothingRenderer.sprite = null;
        equippedItemId = null;
    }

    public string GetEquippedItemId() => equippedItemId;
}
