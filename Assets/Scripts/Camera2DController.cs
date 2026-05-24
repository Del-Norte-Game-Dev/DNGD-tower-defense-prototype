using UnityEngine;

public class Camera2DController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 12f;
    public float moveSmoothTime = 0.12f;

    [Header("Zoom")]
    public float zoomSpeed = 1f;
    public float zoomSmoothTime = 0.12f;
    public float minZoom = 3f;
    public float maxZoom = 20f;

    [Header("Input")]
    public bool allowDragging = false;
    public KeyCode dragButton = KeyCode.Mouse0;

    private Camera cam;

    private Vector3 moveVelocity;
    private float zoomVelocity;
    private Vector3 targetPosition;
    private float targetZoom;
    private Vector3 lastMouseWorldPos;
    private bool isDragging;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        if (!cam.orthographic)
            Debug.LogWarning("Camera2DController is intended for Orthographic cameras.");

        targetPosition = transform.position;
        targetZoom = cam.orthographicSize;
    }

    void Update()
    {
        if (cam == null) return;

        HandleKeyboardMovement();
        if (allowDragging) HandleMouseDrag();
        HandleZoom();

        SmoothMovement();
        SmoothZoom();
    }

    void HandleKeyboardMovement()
    {
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        Vector3 moveDir = (transform.right * input.x + transform.up * input.y);
        targetPosition += moveDir * moveSpeed * Time.unscaledDeltaTime;
    }

    void HandleMouseDrag()
    {
        if (Input.GetKeyDown(dragButton))
        {
            isDragging = true;
            lastMouseWorldPos = GetMouseWorldPosition();
        }

        if (Input.GetKeyUp(dragButton))
        {
            isDragging = false;
        }

        if (!isDragging) return;

        Vector3 currentMouseWorldPos = GetMouseWorldPosition();
        
        Vector3 delta = lastMouseWorldPos - currentMouseWorldPos;
        targetPosition += delta;

        lastMouseWorldPos = currentMouseWorldPos;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.0001f)
        {
            targetZoom -= scroll * zoomSpeed * 100f;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
    }

    void SmoothMovement()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref moveVelocity,
            moveSmoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );
    }

    void SmoothZoom()
    {
        if (!cam.orthographic) return;

        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            targetZoom,
            ref zoomVelocity,
            zoomSmoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = Mathf.Abs(cam.transform.position.z);
        return cam.ScreenToWorldPoint(mouse);
    }
}