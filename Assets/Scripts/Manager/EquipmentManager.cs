using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public SpriteRenderer clothingRenderer;
    public void Equip(ClothingItemData item)
    {
        if (item == null || item.clothingSprite == null) return;
        clothingRenderer.sprite = item.clothingSprite;
    }
}
