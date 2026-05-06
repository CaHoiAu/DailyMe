using System.Collections.Generic;
using UnityEngine;

public class MiniLevelManager : MonoBehaviour
{
    public ConstraintManager constraintManager;
    private GridDragObject[] dragObjects;
    private Slot[] slots;

    private bool puzzleCompleted = false;
    public LevelManager levelManager;

    [Header("Containers")]
    public Transform rackContainer;
    public Transform puzzleContainer;
    [Header("Rack")]
    public RackSnapController rackSnapController;

    private GameObject currentBoard;
    private readonly List<GameObject> spawnedDressupItems = new List<GameObject>();
    public void LoadDressUp(MiniLevelData data)
    {
        if (data == null)
        {
            Debug.LogError("Level data is missing!");
            return;
        }

        if (rackContainer != null)
        {
            rackContainer.localPosition = Vector3.zero;
            rackContainer.localRotation = Quaternion.identity;
        }

        if (currentBoard != null)
        {
            Destroy(currentBoard);
            currentBoard = null;
        }

        foreach (GameObject item in spawnedDressupItems)
        {
            if (item != null) Destroy(item);
        }
        spawnedDressupItems.Clear();

        if (data.boardPrefab == null)
        {
            Debug.LogError("Board prefab is missing!");
            return;
        }

        currentBoard = SpawnBoard(data);

        EquipmentManager character = currentBoard.GetComponent<EquipmentManager>();
        if (character == null)
        {
            Debug.LogError("EquipmentManager is missing on board prefab!");
            return;
        }

        float spacing = 2.5f;
        float startX = -(data.objects.Length - 1) * spacing / 2f;

        for (int i = 0; i < data.objects.Length; i++)
        {
            var objData = data.objects[i];

            if (objData == null || objData.prefab == null)
            {
                Debug.LogError($"Prefab missing at index {i}");
                continue;
            }

            GameObject item = Instantiate(objData.prefab);
            item.transform.SetParent(rackContainer, false);
            item.transform.localPosition = new Vector3(startX + i * spacing, 0, 0);
            item.transform.localRotation = Quaternion.identity;
            item.transform.localScale = Vector3.one;

            DragObject dragObject = item.GetComponent<DragObject>();
            if (dragObject != null)
            {
                dragObject.enabled = false;
            }

            Debug.Log($"{item.name} parent after spawn: {item.transform.parent.name}");

            spawnedDressupItems.Add(item);
        }

        if (rackSnapController != null)
        {
            rackSnapController.equipmentManager = character;
        }
    }
    public void LoadPuzzle(MiniLevelData data)
    {
        ClearPuzzleContainer();
        ClearRackContainer();

        puzzleCompleted = false;

        GameObject board = SpawnBoard(data);

        if (board != null)
            SpawnSlots(data, board.transform);

        SpawnObjects(data);
        List<ICSPConstraint> runtimeConstraints = BuildConstraintData(data.constraints);
        //runtimeConstraints.Add(new CSPOneObjectPerSlotConstraint());
        //runtimeConstraints.Add(new CSPExactSlotConstraint("Toc", "Slot_6"));
        //runtimeConstraints.Add(new CSPExactSlotConstraint("AoQuan", "Slot_5"));
        //runtimeConstraints.Add(new CSPExactSlotConstraint("Giay", "Slot_4"));

        constraintManager.SetConstraints(runtimeConstraints);
    }
    private List<ICSPConstraint> BuildConstraintData(ConstraintData[] constraints)
    {
        List<ICSPConstraint> runtimeConstraints = new List<ICSPConstraint>();
        foreach (var constraintData in constraints)
        {
            switch (constraintData.constraintType)
            {
                //case ConstraintType.ExactSlot:
                //    runtimeConstraints.Add(new CSPExactSlotConstraint(
                //        constraintData.objectA,
                //        constraintData.slotID
                //    ));
                //    break;
                case ConstraintType.Orientation:
                    runtimeConstraints.Add(new CSPOrientationConstraint(
                        constraintData.objectA,
                        constraintData.boolValue
                    ));
                    break;
                case ConstraintType.StackOrder:
                    runtimeConstraints.Add(new CSPStackOrder(
                        constraintData.objectA,
                        constraintData.objectB
                    ));
                    break;
                default:
                    Debug.LogWarning("Unknown constraint type: " + constraintData.constraintType);
                    break;
            }
        }
        return runtimeConstraints;
    }
    GameObject SpawnBoard(MiniLevelData miniLevel)
    {
        if (miniLevel.boardPrefab == null)
        {
            Debug.LogError("Board prefab is missing!");
            return null;
        }
        GameObject board = Instantiate(
            miniLevel.boardPrefab,
            puzzleContainer
        );

        board.transform.position = miniLevel.boardPosition;
        board.transform.rotation = Quaternion.identity;
        board.transform.localScale = Vector3.one;

        Debug.Log("Spawning board: " + board.name);
        return board;
    }
    void SpawnSlots(MiniLevelData miniLevel, Transform boardTransform)
    {
        foreach (var slotData in miniLevel.slots)
        {
            Debug.Log("Spawning slot: " + slotData.slotId);
            GameObject slot = Instantiate(
                slotData.prefab, 
                boardTransform
                );
            slot.transform.localPosition = slotData.position;
            slot.transform.localRotation = Quaternion.identity;
            //slot.transform.localScale = Vector3.one;

            Slot slotScript = slot.GetComponent<Slot>();
            slotScript.slotId = slotData.slotId;
            slotScript.maxLayer = slotData.maxLayer;
        }
    }
    void SpawnObjects(MiniLevelData miniLevel)
    {
        foreach (var objData in miniLevel.objects)
        {
            Debug.Log("Spawning object: " + objData.objectId);
            GameObject obj = Instantiate(
                objData.prefab,
                puzzleContainer
                );
            obj.transform.localPosition = objData.startPosition;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            DragObject drag = obj.GetComponent<DragObject>();
            if (drag == null)
            {
                Debug.LogError($"DragObject component is missing on prefab for object {objData.objectId}!");
                continue;
            }
            drag.enabled = true; // Enable dragging for puzzle objects
            drag.objectId = objData.objectId;
            drag.startPosition = objData.startPosition;
            drag.miniLevelManager = this;
        }

        dragObjects = puzzleContainer.GetComponentsInChildren<GridDragObject>();
        slots = puzzleContainer.GetComponentsInChildren<Slot>();
    }
    void ClearPuzzleContainer()
    {
        foreach (Transform child in puzzleContainer)
        {
            Destroy(child.gameObject);
        }
        currentBoard = null;
        dragObjects = null;
        slots = null;
    }
    void ClearRackContainer()
    {
        foreach (Transform child in rackContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedDressupItems.Clear();
    }
    public void OnObjectChanged()
    {
        if (puzzleCompleted)
            return;
        CheckPuzzleComplete();
    }
    bool AreAllObjectsPlaced()
    {
        if (dragObjects == null || dragObjects.Length == 0)
            return false;
        foreach (var obj in dragObjects)
        {
            if (obj == null)
                return false;

        }
        return true;
    }
    public void CheckPuzzleComplete()
    {
        if (!AreAllObjectsPlaced())
            return;
        if (!constraintManager.CheckAllConstraints(dragObjects))
            return;
        puzzleCompleted = true;
        Debug.Log("Puzzle completed!");
        levelManager.NextMiniGame();
    }
    public void ResetPuzzle()
    {
        Debug.Log("Resetting puzzle...");
        foreach (var obj in dragObjects)
        {
                obj.ReturnToStart();
        }
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
        puzzleCompleted = false;
        Debug.Log("Puzzle reset complete.");
    }
    public bool ValidateCurrentState()
    {
        return constraintManager.CheckAllConstraints(dragObjects);
    }
}
