using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class ContainedObjectDisplay
{
    public string objectId;
    public Vector3 positionOffset = Vector3.zero;
    public float scale = 1f;
    public int sortingOrderOffset = 1;
}

[System.Serializable]
public class GridObjectState
{
    public int stateId;
    public string stateName;
    public Sprite sprite;
    public Vector2Int[] cells;
    public bool allowRotate = true;

    [Header("Container")]
    public bool isContainer = false;
    public string[] containedObjectIds = new string[0];

    [Header("Display Settings")]
    public ContainedObjectDisplay[] containedObjectDisplays = new ContainedObjectDisplay[0];
}

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class GridDragObject : MonoBehaviour
{
    public string objectId;
    public GridObjectState[] states;
    public int currentStateIndex = 0;

    public Vector3 startWorldPosition;
    public bool isPlaced = false;

    public int currentRotationIndex = 0;
    public Vector2Int currentGridPosition;
    public int currentLayer = 0; // For potential layering in constraints

    [Header("Inspect")]
    public Sprite inspectSprite;

    [Header("Containment")]
    private List<GridDragObject> containedObjects = new List<GridDragObject>();
    private Transform containmentDisplayParent;
    public bool isContainedObject = false;
    public GridDragObject currentContainer = null; // ✅ Fix: Track current container instead of relying on transform.parent

    [Header("Contain Display Settings")]
    private bool wasInContainerBeforeDrag = false;
    private GridDragObject previousContainer = null;

    public bool hasHomeDisplayPosition = false;
    public Vector3 homeDisplayWorldPosition;
    public Vector3 homeDisplayScale = Vector3.one;

    private Camera cam;
    private bool isDragging = false;
    private bool hasDragged = false;
    private Vector3 offset;
    private Vector3 mouseDownPositon;
    private SpriteRenderer spriteRenderer;
    private Collider2D objectCollider;
    private bool isFullyInitialized = false;

    public GridPuzzleManager gridPuzzleManager;

    //Drop effect configuration
    private float dropDuration = 2f;
    private float dropHeight = 2f;
    private AnimationCurve dropAnimationCurve;
    private Coroutine dropCoroutine;

    public GridObjectData objectData;

    private bool isTopObject = false;
    public bool isLocked = false;

    public Vector2Int[] forbiddenCells = new Vector2Int[0]; //cells this object can never be placed on

    public void ResetToDefaultState()
    {
        currentStateIndex = 0;
        currentRotationIndex = 0;
        currentContainer = null;
        isContainedObject = false;
        hasHomeDisplayPosition = false;
        containedObjects.Clear();

        ApplyCurrentVisual();
        SetVisible(true);
        if (objectCollider != null) objectCollider.enabled = true;
    }

    public void Initialize()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectCollider = GetComponent<Collider2D>();
        startWorldPosition = transform.position;

        containedObjects.Clear();
        currentContainer = null;
        isContainedObject = false;
        hasHomeDisplayPosition = false;

        // ✅ THÊM: Reset về state đầu tiên
        currentStateIndex = 0;
        currentRotationIndex = 0;

        SetUpContainmentDisplay();
        ApplyCurrentVisual();

        isFullyInitialized = false;
    }

    public void FinalizeRuntimeSetup()
    {
        Debug.Log($"[{objectId}] FinalizeRuntimeSetup START - currentStateIndex={currentStateIndex}");

        RegisterHomePositions();
        isFullyInitialized = true;

        Debug.Log($"[{objectId}] Calling UpdateContainedObjectsDisplay()");
        UpdateContainedObjectsDisplay();

        Debug.Log($"[{objectId}] FinalizeRuntimeSetup END");
    }

    public void SetHomeDisplayPosition(Vector3 worldPos, Vector3 scale)
    {
        hasHomeDisplayPosition = true;
        homeDisplayScale = scale;
        homeDisplayWorldPosition = worldPos;
    }

    public void ReturnToHomeDisplayPosition()
    {
        if (!hasHomeDisplayPosition) return;

        transform.position = homeDisplayWorldPosition;
        transform.localScale = homeDisplayScale;
        transform.localRotation = Quaternion.identity;

        isPlaced = false;
        currentGridPosition = Vector2Int.zero;
    }

    // ✅ Đổi hàm quét: Quét TẤT CẢ các state để lấy config chứa, tránh kẹt khi Container đang "đóng hộp"
    private ContainedObjectDisplay GetDisplayConfigFromAnyState(string id)
    {
        if (states == null) return null;
        foreach (var state in states)
        {
            if (state.isContainer && state.containedObjectDisplays != null)
            {
                foreach (var display in state.containedObjectDisplays)
                {
                    if (display != null && display.objectId == id)
                    {
                        return display;
                    }
                }
            }
        }
        return null;
    }

    public void RegisterHomePositions()
    {
        if (containmentDisplayParent == null) return;

        // ✅ THAY THẾ: Chỉ lấy objects từ puzzle container hiện tại
        GridDragObject[] allObjects = gridPuzzleManager != null
            ? gridPuzzleManager.puzzleContainer.GetComponentsInChildren<GridDragObject>()
            : FindObjectsOfType<GridDragObject>();

        foreach (var obj in allObjects)
        {
            if (obj == null || obj.currentContainer != this) continue;

            var config = GetDisplayConfigFromAnyState(obj.objectId);
            if (config != null)
            {
                Vector3 worldPos = containmentDisplayParent.TransformPoint(config.positionOffset);
                obj.SetHomeDisplayPosition(worldPos, new Vector3(config.scale, config.scale, 1f));

                Debug.Log($"[RegisterHomePositions] Set home position for [{obj.objectId}] at container [{this.objectId}]");
            }
        }
    }

    private void SetUpContainmentDisplay()
    {
        Transform existingDisplay = transform.Find("ContainmentDisplay");
        if (existingDisplay != null)
        {
            containmentDisplayParent = existingDisplay;
        }
        else
        {
            GameObject displayObj = new GameObject("ContainmentDisplay");
            displayObj.transform.SetParent(transform);
            displayObj.transform.localPosition = Vector3.zero;
            displayObj.transform.localRotation = Quaternion.identity;
            displayObj.transform.localScale = Vector3.one;
            containmentDisplayParent = displayObj.transform;
        }
    }

    public GridObjectState GetCurrentState()
    {
        if (states == null || states.Length == 0) return null;
        return states[currentStateIndex];
    }
    public int CurrentStateId => GetCurrentState()?.stateId ?? -1;
    public Vector2Int[] GetCurrentShape()
    {
        GridObjectState currentState = GetCurrentState();
        if (currentState == null) return new Vector2Int[0];
        return ShapeUtils.RotateShape(currentState.cells, currentRotationIndex);
    }

    public Vector2Int[] GetOccupiedCellsAt(Vector2Int gridPos)
    {
        Vector2Int[] shape = GetCurrentShape();
        return ShapeUtils.TranslateCells(shape, gridPos.x, gridPos.y);
    }

    public bool CanRotate()
    {
        GridObjectState currentState = GetCurrentState();
        if (currentState == null) return false;
        return currentState.allowRotate;
    }

    public void NextState()
    {
        // ❌ Remove container checking because we don't want to change state if placed no matter the setup
        if (isPlaced)
        {
            Debug.Log("Cannot change state while placed on board");
            return;
        }

        if (states == null || states.Length == 0) return;

        currentStateIndex = (currentStateIndex + 1) % states.Length;
        currentRotationIndex = 0;
        ApplyCurrentVisual();

        GridObjectState newState = GetCurrentState();
        SyncContainedObjectsWithState(newState);

        UpdateContainedObjectsDisplayInternal();
        RegisterHomePositions();
    }

    private void SyncContainedObjectsWithState(GridObjectState state)
    {
        if (state == null) return;
        containedObjects.Clear();
        foreach (string id in state.containedObjectIds)
        {
            GridDragObject obj = FindObjectById(id);
            if (obj != null)
            {
                containedObjects.Add(obj);
                obj.currentContainer = this;
                obj.SetVisible(true);
            }
        }
    }

    private GridDragObject FindObjectById(string id)
    {
        GridDragObject[] allObjects = FindObjectsOfType<GridDragObject>();
        foreach (var obj in allObjects)
        {
            if (obj.objectId == id) return obj;
        }
        return null;
    }

    public void ApplyCurrentVisual()
    {
        GridObjectState state = GetCurrentState();
        if (state == null || spriteRenderer == null) return;
        spriteRenderer.sprite = state.sprite;
        transform.rotation = Quaternion.Euler(0, 0, 90f * currentRotationIndex);
    }

    public void NextRotation()
    {
        if (!CanRotate()) return;
        currentRotationIndex = (currentRotationIndex + 1) % 4;
        transform.rotation = Quaternion.Euler(0, 0, 90f * currentRotationIndex);
    }

    private void OnMouseDown()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.isPaused) return;
        if (isLocked) return;
        if (!IsTopObjectAtMousePosition()) return;
        isTopObject = true;
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        mouseDownPositon = mouseWorldPos;
        offset = transform.position - mouseWorldPos;
        isDragging = false;
        hasDragged = false;

        wasInContainerBeforeDrag = false;
        previousContainer = null;

        if (currentContainer != null)
        {
            wasInContainerBeforeDrag = true;
            previousContainer = currentContainer;
        }
    }

    private void OnMouseDrag()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.isPaused) return;
        if (isLocked) return;
        if (!isTopObject) return;
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        if (!hasDragged)
        {
            float distance = Vector3.Distance(mouseDownPositon, mouseWorldPos);

            if (distance >= 0.15f)
            {
                hasDragged = true;
                isDragging = true;

                if (gridPuzzleManager != null)
                {
                    gridPuzzleManager.RemoveObjectFromBoard(this);
                }

                // ✅ Removes from current container list visually & logically
                if (currentContainer != null)
                {
                    currentContainer.RemoveContainedObject(this);
                }
            }
        }

        if (isDragging)
        {
            transform.position = mouseWorldPos + offset;
        }
    }

    private void OnMouseUp()
    {
        if (isLocked) return;
        if (!isTopObject) return;
        isTopObject = false;
        if (!hasDragged)
        {
            if (inspectSprite != null && InspectPopupManager.Instance != null)
            {
                InspectPopupManager.Instance.Show(inspectSprite);
            }
            else
            {
                NextState();
                if (gridPuzzleManager != null)
                    gridPuzzleManager.OnObjectChanged();
            }
            return;
        }

        isDragging = false;

        if (gridPuzzleManager == null)
        {
            transform.position = startWorldPosition;
            return;
        }

        Vector2Int gridPos = gridPuzzleManager.boardManager.WorldToGrid(transform.position);
        bool success = gridPuzzleManager.TryPlaceObject(this, gridPos);

        if (!success)
        {
            // ✅ Return logic
            if (wasInContainerBeforeDrag && previousContainer != null)
            {
                previousContainer.AddContainedObject(this);
            }
            else if (hasHomeDisplayPosition)
            {
                ReturnToHomeDisplayPosition();
            }
            else if (isPlaced)
            {
                transform.position = gridPuzzleManager.boardManager.GridToWorld(currentGridPosition);
            }
            else
            {
                transform.position = startWorldPosition;
            }
        }
        gridPuzzleManager.OnObjectChanged();
    }

    public void SnapToGrid(Vector2Int gridPos)
    {
        currentGridPosition = gridPos;
        Vector2Int[] shape = GetCurrentShape();

        // ✅ Tính tâm của toàn bộ cells trong world space
        Vector3 worldCenter = Vector3.zero;
        foreach (var cell in shape)
        {
            Vector3 cellWorld = gridPuzzleManager.boardManager.GridToWorld(
                new Vector2Int(gridPos.x + cell.x, gridPos.y + cell.y)
            );
            worldCenter += cellWorld;
        }
        worldCenter /= shape.Length;

        transform.position = worldCenter;
        isPlaced = true;
    }

    public Vector2 GetShapeCenter(Vector2Int[] cells)
    {
        Vector2 sum = Vector2.zero;
        foreach (var cell in cells) sum += new Vector2(cell.x, cell.y);
        return sum / cells.Length;
    }

    public void ReturnToStart()
    {
        isPlaced = false;
        transform.position = startWorldPosition;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -cam.transform.position.z;
        return cam.ScreenToWorldPoint(mouseScreen);
    }

    public void PlayDropEffect(Vector3 targetPosition)
    {
        if (dropCoroutine != null) StopCoroutine(dropCoroutine);
        dropCoroutine = StartCoroutine(DropCoroutine(targetPosition));
    }

    public void SetDropEffectSettings(float duration, float height, AnimationCurve curve)
    {
        dropDuration = duration;
        dropHeight = height;
        dropAnimationCurve = curve;
    }

    private IEnumerator DropCoroutine(Vector3 targetPosition)
    {
        Vector3 startPos = transform.position + Vector3.up * dropHeight;
        transform.position = startPos;
        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropDuration);
            float curveValue = dropAnimationCurve != null ? dropAnimationCurve.Evaluate(t) : t;
            transform.position = Vector3.Lerp(startPos, targetPosition, curveValue);
            yield return null;
        }
        transform.position = targetPosition;
    }

    public int GetLayer() => currentLayer;

    public void SetLayer(int layer)
    {
        currentLayer = layer;
        if (spriteRenderer != null) spriteRenderer.sortingOrder = layer + 10;
    }

    public void SetVisible(bool visible)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = visible;
        if (objectCollider != null) objectCollider.enabled = visible;
    }

    public void AddContainedObject(GridDragObject obj)
    {
        if (obj == null) return;
        if (containedObjects.Contains(obj)) return;

        // ✅ Quét tất cả List configs (thay vì chỉ config chứa trong State hiện tại).
        // Qua đó GridPuzzleManager thả Data vào hợp pháp kể cả khi đang đóng.
        bool isAllowed = false;
        if (states != null)
        {
            foreach (var state in states)
            {
                if (state.isContainer && state.containedObjectIds != null)
                {
                    if (System.Array.Exists(state.containedObjectIds, id => id == obj.objectId))
                    {
                        isAllowed = true;
                        break;
                    }
                }
            }
        }

        if (!isAllowed) return;

        containedObjects.Add(obj);
        obj.isPlaced = false;
        obj.isContainedObject = true;
        obj.currentContainer = this; // ✅ Nối liên kết

        ContainedObjectDisplay display = GetDisplayConfigFromAnyState(obj.objectId);
        if (display != null && containmentDisplayParent != null)
        {
            Vector3 worldPos = containmentDisplayParent.TransformPoint(display.positionOffset);
            Vector3 scale = new Vector3(display.scale, display.scale, 1f);

            obj.SetHomeDisplayPosition(worldPos, scale);
            obj.ReturnToHomeDisplayPosition();

            if (obj.spriteRenderer != null && spriteRenderer != null)
            {
                obj.spriteRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
                obj.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 100 + containedObjects.Count + display.sortingOrderOffset;
            }
        }

        UpdateContainedObjectsDisplayInternal();
    }

    public void RemoveContainedObject(GridDragObject obj)
    {
        containedObjects.Remove(obj);
        obj.currentContainer = null;

        obj.SetVisible(true);
        if (obj.objectCollider != null)
        {
            obj.objectCollider.enabled = true;
        }
        UpdateContainedObjectsDisplayInternal();
    }

    public List<GridDragObject> GetContainedObjects() => new List<GridDragObject>(containedObjects);

    public void UpdateContainedObjectsDisplay()
    {
        UpdateContainedObjectsDisplayInternal();
    }

    // ✅ THAY THẾ UpdateContainedObjectsDisplayInternal() bằng logic đơn giản
    private void UpdateContainedObjectsDisplayInternal()
    {
        Debug.Log($"[{objectId}] UpdateContainedObjectsDisplayInternal - State[{currentStateIndex}]");

        if (containmentDisplayParent == null)
        {
            Debug.LogWarning($"[{objectId}] containmentDisplayParent is NULL!");
            return;
        }

        GridObjectState currentState = GetCurrentState();
        if (currentState == null)
        {
            Debug.LogWarning($"[{objectId}] currentState is NULL!");
            return;
        }

        Debug.Log($"[{objectId}] currentState.isContainer={currentState.isContainer}");

        GridDragObject[] allObjects = gridPuzzleManager != null
            ? gridPuzzleManager.puzzleContainer.GetComponentsInChildren<GridDragObject>()
            : FindObjectsOfType<GridDragObject>();

        Debug.Log($"[{objectId}] Found {allObjects.Length} objects in puzzle container");

        foreach (var obj in allObjects)
        {
            if (obj == null || obj.currentContainer != this) continue;

            bool isInCurrentState = currentState.isContainer &&
                                     System.Array.Exists(currentState.containedObjectIds, id => id == obj.objectId);

            Debug.Log($"  [{obj.objectId}] isInCurrentState={isInCurrentState}");
            obj.SetVisible(isInCurrentState);

            if (!isInCurrentState) continue;

            ContainedObjectDisplay displayConfig = GetDisplayConfigFromAnyState(obj.objectId);
            if (displayConfig != null && containmentDisplayParent != null)
            {
                Vector3 worldPos = containmentDisplayParent.TransformPoint(displayConfig.positionOffset);
                obj.transform.position = worldPos;
                obj.transform.localScale = new Vector3(displayConfig.scale, displayConfig.scale, 1f);
                obj.transform.localRotation = Quaternion.identity;

                if (obj.spriteRenderer != null && spriteRenderer != null)
                {
                    obj.spriteRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
                    obj.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 100 + displayConfig.sortingOrderOffset;
                }

                Debug.Log($"    ✅ Updated position to {worldPos}");
            }
        }
    }

    public void EmptyContainer()
    {
        List<GridDragObject> objectsToRemove = new List<GridDragObject>(containedObjects);
        foreach (var obj in objectsToRemove)
        {
            RemoveContainedObject(obj);
        }
    }

    private bool IsTopObjectAtMousePosition()
    {
        // ✅ Dùng ScreenToWorldPoint đúng cách cho 2D
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -cam.transform.position.z;
        Vector2 mousePos = cam.ScreenToWorldPoint(mouseScreen);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        if (hits.Length == 0) return false;

        Collider2D topCollider = null;
        int highestOrder = int.MinValue;
        int highestSiblingIndex = int.MinValue;

        foreach (var hit in hits)
        {
            var sr = hit.collider.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            int siblingIndex = hit.collider.transform.GetSiblingIndex();

            if (sr.sortingOrder > highestOrder ||
               (sr.sortingOrder == highestOrder && siblingIndex > highestSiblingIndex))
            {
                highestOrder = sr.sortingOrder;
                highestSiblingIndex = siblingIndex;
                topCollider = hit.collider;
            }
        }

        return topCollider != null && topCollider == objectCollider;
    }
}