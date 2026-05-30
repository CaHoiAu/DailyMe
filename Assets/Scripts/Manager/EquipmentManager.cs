using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public SpriteRenderer clothingRenderer;

    private string equippedItemId;

    public void Equip(ClothingItemData item)
    {
        if (item == null || item.clothingSprite == null) return;
        clothingRenderer.sprite = item.clothingSprite;
        equippedItemId = item.itemId;
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
