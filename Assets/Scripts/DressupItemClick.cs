using UnityEngine;

public class DressupItemClick : MonoBehaviour
{
    public ClothingItemData clothingItemData;
    public EquipmentManager equipmentManager;

    private void OnMouseDown()
    {
        if (clothingItemData != null && equipmentManager != null)
        {
            equipmentManager.Equip(clothingItemData);
        }
    }
}
