using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public string boardId;
    public List<Slot> allSlots;
    private void Awake()
    {
        allSlots = new List<Slot>(GetComponentsInChildren<Slot>());
    }
    public Slot GetNearestSlot(Vector3 position)
    {
        float minDist = float.MaxValue;
        Slot nearest = null;

        foreach (Slot slot in allSlots)
        {
            float dist = Vector3.Distance(position, slot.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = slot;
            }
        }
        return nearest;
    }
    public Slot GetSlotById(string slotId)
    {
        return allSlots.Find(slot => slot.slotId == slotId);
    }
    public bool AreAdjacent(Slot a, Slot b)
    {
        if (a == null || b == null) return false;
        float distance = Vector2.Distance(a.transform.position, b.transform.position);
        return distance < 1.2f; // Giả sử khoảng cách giữa các slot là 1 đơn vị, điều chỉnh nếu cần
    }
}
