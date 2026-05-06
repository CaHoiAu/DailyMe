using UnityEditor.U2D.Aseprite;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DragObject : MonoBehaviour
{
    public string objectId;
    public string objectType;

    [Header("State List")]
    [SerializeField] private ObjectVisualState[] states;
    [SerializeField] private int currentStateIndex = 0;

    private SpriteRenderer spriteRenderer;
    private Camera cam;

    private Vector3 offset;
    private bool isDragging = false;
    private bool hasDragged = false;
    private Vector3 returnPosition;
    public Vector3 startPosition;
    public Slot currentSlot;

    private Vector3 mouseDownPosition;

    [Header("Raycast Settings")]
    public LayerMask slotLayerMask;

    [Header("Drag Settings")]
    [SerializeField] private float dragThreshold = 0.2f;
    
    public LevelManager levelManager;
    public MiniLevelManager miniLevelManager;
    public bool IsVertical()
    {
        if (states == null || states.Length == 0) return false;
        return states[currentStateIndex].isVertical;
    }
    public int GetWidth()
    {
       if (states == null || states.Length == 0) return 1;
       return states[currentStateIndex].width;
    }
    public int GetHeight()
    {
        if (states == null || states.Length == 0) return 1;
        return states[currentStateIndex].height;
    }
    private void Awake()
    {
        cam = Camera.main;
        startPosition = transform.position;
        returnPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        levelManager = FindObjectOfType<LevelManager>();
        ApplyCurrentState();
    }
    private void ApplyCurrentState()
    {
        if (states == null || states.Length == 0 ||spriteRenderer == null) return;
        spriteRenderer.sprite = states[currentStateIndex].sprite;
    }
    public void NextState()
    {
        if (states == null || states.Length == 0) return;
        currentStateIndex++;
        if (currentStateIndex >= states.Length)
            currentStateIndex = 0;
        ApplyCurrentState();
    }
    private void OnMouseDown()
    {
        Vector3 mouseWorld = GetMouseWorld();
        mouseDownPosition = mouseWorld;
        offset = transform.position - mouseWorld;
        hasDragged = false;
        isDragging = false;
        returnPosition = transform.position;
    }
    private void OnMouseDrag()
    {
        Vector3 mouseWorld = GetMouseWorld();

        if(!hasDragged)
        {
            float distance = Vector3.Distance(mouseDownPosition, mouseWorld);
            if (distance >= dragThreshold)
            {
                isDragging = true;
                hasDragged = true;

                if(currentSlot != null)
                {
                    currentSlot.RemoveObject(this);
                    currentSlot = null;
                }
            }
        }
        if (isDragging)
        {
            transform.position = mouseWorld + offset;
        }
    }
    private void OnMouseUp()
    {
        if (!hasDragged)
        {
            NextState();
            if (miniLevelManager != null)
                miniLevelManager.OnObjectChanged();
            return;
        }

        isDragging = false;
        Slot slot = GetSlotUnderObject();

        if (slot != null)
        {
            Debug.Log("Hit slot: " + slot.slotId + " | Object: " + objectId);
            Debug.Log("Can fit: " + slot.CanFitObject(this));
            Debug.Log("Object size: " + GetWidth() + "x" + GetHeight());
            Debug.Log("Slot size: " + slot.slotWidth + "x" + slot.slotHeight);
        }
        else
        {
            Debug.Log("No slot hit. Returning to start position.");
        }

        bool allowedByConstraint = true;

        //if (slot != null && miniLevelManager != null && miniLevelManager.constraintManager != null)
        //{
        //    allowedByConstraint = miniLevelManager.constraintManager.IsPlacementAllowed(this, slot);
        //    Debug.Log("Allowed by constraint: " + allowedByConstraint);
        //}

        if (slot != null && slot.CanPlaceObject(this) && allowedByConstraint)
        {
            int nextLayer = slot.GetNextLayer();

            transform.position = slot.GetSnapPositionForLayer(nextLayer);
            currentSlot = slot;
            slot.PlaceObject(this);

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = nextLayer + 10;
        }
        else
        {
            transform.position = startPosition;
        }

        if (miniLevelManager != null)
            miniLevelManager.OnObjectChanged();
    }
    private Vector3 GetMouseWorld()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -cam.transform.position.z; // Đảm bảo z đúng để raycast chính xác
        return cam.ScreenToWorldPoint(mouseScreen);
    }
    private Slot GetSlotUnderObject()
    {
        Vector2 worldPoint = transform.position;
        float radius = 0.3f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPoint, radius, slotLayerMask);
        
        foreach (var hit in hits)
        {
            Debug.Log("Hit collider: " + hit.name);
            Slot slot = hit.GetComponentInParent<Slot>();
            if (slot != null)
            {
                Debug.Log("Found slot: " + slot.slotId + " under object: " + objectId);
                return slot;
            }
        }
        Debug.Log("No collider hit at position: " + worldPoint);
        return null;
    }
    public string GetCurrentSlotId()
    {
        return currentSlot != null ? currentSlot.slotId : null;
    }
    public void ReturnToStartPosition()
    {
        if (currentSlot != null)
        {
            currentSlot.RemoveObject(this);
            currentSlot = null;
        }
        transform.position = startPosition;
        //ApplyCurrentState();
    }
}
