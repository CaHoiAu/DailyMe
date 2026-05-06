using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LayerPoint
{
    [UnityEngine.Range(0f, 1f)] public float x = 0.5f;
    [UnityEngine.Range(0f, 1f)] public float y = 0.5f;
}

[RequireComponent(typeof(Collider2D))]
public class Slot : MonoBehaviour
{
    public string slotId = "SLOT_01";
    public Transform snapPoint;

    [Header("Layer Settings")]
    public int maxLayer;
    public LayerPoint[] layerSnapPoints;

    private List<DragObject> placedObjects = new List<DragObject>();

    private Collider2D slotCollider;

    [Header("Size Settings")]
    [SerializeField] public int slotWidth = 2;
    [SerializeField] public int slotHeight = 2;

    private void Awake()
    {
        slotCollider = GetComponent<Collider2D>();
    }
    public bool CanFitObject(DragObject obj)
    {
        if (obj == null) return false;
        return obj.GetWidth() <= slotWidth && obj.GetHeight() <= slotHeight;
    }
    public bool HasSpace()
    {
        return placedObjects.Count < maxLayer;
    }
    public bool CanPlaceObject(DragObject obj)
    {
        return CanFitObject(obj) && HasSpace();
    }
    public int GetNextLayer()
    {
        return placedObjects.Count + 1;
    }
    public void PlaceObject(DragObject obj)
    {
        if(!placedObjects.Contains(obj))
            placedObjects.Add(obj);
    }
    public void RemoveObject(DragObject obj)
    {
        if (placedObjects.Contains(obj))
            placedObjects.Remove(obj);
    }
    public void ClearSlot()
    {
        placedObjects.Clear();
    }
    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
        if(snapPoint == null)
        {
            var t = transform.Find("SnapPoint");
            if (t != null) snapPoint = t;
        }
    }
    public Vector3 GetSnapPosition()
    {
        Vector3 basePos = snapPoint != null ? snapPoint.position : transform.position;
        float layerOffset = 0.1f; // Khoảng cách giữa các layer
        return basePos + new Vector3(0, layerOffset * placedObjects.Count, 0);
    }
    public Vector3 GetSnapPositionForLayer(int layer)
    {
        int index = layer - 1;
        if (slotCollider == null) 
            return snapPoint != null ? snapPoint.position : transform.position;
        Bounds bounds = slotCollider.bounds;
        if (layerSnapPoints != null && index >= 0 && index < layerSnapPoints.Length && layerSnapPoints[index] != null)
        {
            float worldX = Mathf.Lerp(bounds.min.x, bounds.max.x, layerSnapPoints[index].x);
            float worldY = Mathf.Lerp(bounds.min.y, bounds.max.y, layerSnapPoints[index].y);
            return new Vector3 (worldX, worldY, transform.position.z);
        }
        return snapPoint != null ? snapPoint.position : transform.position;
    }
    public int GetCurrentLayer(DragObject obj)
    {
        int index = placedObjects.IndexOf(obj);
        return index >= 0 ? index + 1 : -1;
    }
}
