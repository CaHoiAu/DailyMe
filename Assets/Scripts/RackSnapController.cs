using System.Collections;
using UnityEngine;

public class RackSnapController : MonoBehaviour
{
    public Transform centerMarker;
    public EquipmentManager equipmentManager;
    [Tooltip("The transform that drag moves (usually RackDragController's transform). Leave null to use this transform.")]
    public Transform rackMoveTarget;

    public float snapSpeed = 8f;
    private bool isSnapping = false;

    public void SnapToNearestItem(bool equip = true)
    {
        if (isSnapping) return;

        ClothingItemData nearestItem = FindNearestItem();
        if (nearestItem != null)
        {
            StartCoroutine(SmoothSnap(nearestItem.transform, equip));
        }
    }
    ClothingItemData FindNearestItem()
    {
        Transform searchRoot = transform.root;
        ClothingItemData[] items = searchRoot.GetComponentsInChildren<ClothingItemData>();

        if (items.Length == 0)
            Debug.LogWarning("[RackSnapController] No ClothingItemData found under root: " + searchRoot.name);
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
    IEnumerator SmoothSnap(Transform targetItem, bool equip = true)
    {
        isSnapping = true;
        Transform moveTarget = rackMoveTarget != null ? rackMoveTarget : transform;

        float offsetX = centerMarker.position.x - targetItem.position.x;
        Vector3 targetRackPosition = moveTarget.position + new Vector3(offsetX, 0, 0);

        Debug.Log($"[RackSnapController] Snapping to: {targetItem.name}, offsetX: {offsetX:F2}, equip: {equip}");

        while (Vector3.Distance(moveTarget.position, targetRackPosition) > 0.01f)
        {
            moveTarget.position = Vector3.Lerp(moveTarget.position, targetRackPosition, Time.deltaTime * snapSpeed);
            yield return null;
        }

        moveTarget.position = targetRackPosition;

        if (equip)
        {
            ClothingItemData itemData = targetItem.GetComponent<ClothingItemData>();
            if (equipmentManager == null)
                Debug.LogError("[RackSnapController] equipmentManager is NULL — forgot to wire in DressUpGameManager.SpawnRackSystem?");
            else if (itemData == null)
                Debug.LogError("[RackSnapController] ClothingItemData not found on snapped item: " + targetItem.name);
            else
                equipmentManager.Equip(itemData);
        }
        isSnapping = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
