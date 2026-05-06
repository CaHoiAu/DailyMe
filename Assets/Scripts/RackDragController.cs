using UnityEngine;
using UnityEngine.UIElements;

public class RackDragController : MonoBehaviour
{
    private Vector3 mouseDownScreenPos;
    private Vector3 lastMouseScreenPos;
    private Vector3 lastMouseWorldPos;

    public RackSnapController snapController;

    [Header("Drag Settings")]
    public float dragSensitivity = 0.01f;
    public float dragThreshold = 15f;
    public bool useWorldSpaceDrag = false;

    private bool isPointerDown;
    private bool isDragging;

    // Update is called once per frame
    void Update()
    {
        HandleDrag();
    }
    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isPointerDown = true;
            isDragging = false;

            lastMouseWorldPos = GetMouseWorldPosition();
            mouseDownScreenPos = Input.mousePosition;
            lastMouseScreenPos = mouseDownScreenPos;
            Debug.Log("Mouse down at: " + mouseDownScreenPos);
        }
        if (Input.GetMouseButton(0) && isPointerDown)
        {
            Vector3 currentMouseScreenPos = Input.mousePosition;

            if (!isDragging)
            {
                float movedDistance = Vector2.Distance(currentMouseScreenPos, mouseDownScreenPos);
                if (movedDistance >= dragThreshold)
                {
                    isDragging = true;
                    Debug.Log("Started dragging after moving: " + movedDistance);
                }
            }
            if (isDragging)
            {
                if (useWorldSpaceDrag)
                {
                    Vector3 currentWorldPos = GetMouseWorldPosition();
                    float deltaX = currentWorldPos.x - lastMouseWorldPos.x;

                    transform.position += new Vector3(deltaX, 0f, 0f);
                    lastMouseWorldPos = currentWorldPos;
                }
                else
                {
                    float deltaX = currentMouseScreenPos.x - lastMouseScreenPos.x;

                    transform.localPosition += new Vector3(deltaX * dragSensitivity, 0f, 0f);
                    lastMouseScreenPos = currentMouseScreenPos;
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (isPointerDown && isDragging)
            {
                if (snapController != null)
                {
                    snapController.SnapToNearestItem();
                }
            }
            isPointerDown = false;
            isDragging = false;
            Debug.Log("Mouse up at: " + Input.mousePosition);
        }
    }
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }
}
