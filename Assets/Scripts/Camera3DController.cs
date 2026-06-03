using UnityEngine;

public class Camera3DController : MonoBehaviour
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

    public float pitchAngle = -20f;
    public float cameraHeight = 20f;
    public Plane groundPlane;

    private Camera cam;

    private Vector3 moveVelocity;
    private float zoomVelocity;
    private Vector3 targetPosition;
    private float targetFOV;
    private Vector3 lastMouseWorldPos;
    private bool isDragging;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        if (cam.orthographic)
            Debug.LogWarning("Camera3DController is intended for Perspective cameras.");

        transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);

        targetPosition = transform.position;
        targetFOV = cam.fieldOfView;

        groundPlane = new Plane(Vector3.forward, Vector3.zero);
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

        Vector3 moveDir = new Vector3(input.x, input.y, 0);
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
        delta.z = 0;
        targetPosition += delta;

        lastMouseWorldPos = currentMouseWorldPos;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.0001f)
        {
            targetFOV -= scroll * zoomSpeed * 100f;
            targetFOV = Mathf.Clamp(targetFOV, minZoom, maxZoom);
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
        if (cam.orthographic) return;

        cam.fieldOfView = Mathf.SmoothDamp(
            cam.fieldOfView,
            targetFOV,
            ref zoomVelocity,
            zoomSmoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (groundPlane.Raycast(ray, out float distance))
            return ray.GetPoint(distance);

        return Vector3.zero;
    }
}
