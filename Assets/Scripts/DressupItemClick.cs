using UnityEngine;

public class DressupItemClick : MonoBehaviour
{
    public ClothingItemData clothingItemData;
    public EquipmentManager equipmentManager;

    private void OnMouseDown()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.isPaused)
            return; // Ignore clicks when the game is paused)
        if (clothingItemData != null && equipmentManager != null)
        {
            equipmentManager.Equip(clothingItemData);
        }
    }
}
