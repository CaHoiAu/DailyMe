using System.Collections;
using UnityEngine;

public class RackSnapController : MonoBehaviour
{
    public Transform centerMarker;
    public EquipmentManager equipmentManager;

    public float snapSpeed = 8f;
    private bool isSnapping = false;

    public void SnapToNearestItem()
    {
        if (isSnapping) return;

        ClothingItemData nearestItem = FindNearestItem();
        if (nearestItem != null)
        {
            StartCoroutine(SmoothSnap(nearestItem.transform));
        }
    }
    ClothingItemData FindNearestItem()
    {
        ClothingItemData[] items = GetComponentsInChildren<ClothingItemData>();

        ClothingItemData nearestItem = null;
        float minDistance = Mathf.Infinity;

        foreach (ClothingItemData item in items)
        {
            float distance = Vector3.Distance(centerMarker.position, item.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestItem = item;
            }
        }
        return nearestItem;
    }
    IEnumerator SmoothSnap(Transform targetItem)
    {
        isSnapping = true;
        float offsetX = centerMarker.position.x - targetItem.position.x;
        Vector3 targetRackPosition = transform.position + new Vector3(offsetX, 0, 0);

        while (Vector3.Distance(transform.position, targetRackPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetRackPosition, Time.deltaTime * snapSpeed);
            yield return null;
        }

        transform.position = targetRackPosition;

        ClothingItemData itemData = targetItem.GetComponent<ClothingItemData>();
        if(equipmentManager != null && itemData != null)
        {
            equipmentManager.Equip(itemData);
        }
        isSnapping = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
