using UnityEngine;

public class FlowFieldFollower : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float directionRefreshInterval = 0.12f;

    private EntityMovement2D movement;
    private EntityController2D controller;
    private FlowField flowField;

    private Vector2 currentFlowDirection;
    private float directionRefreshTimer;
    private bool selected;

    private void Awake()
    {
        movement = GetComponent<EntityMovement2D>();
        controller = GetComponent<EntityController2D>();
    }

    private void Start()
    {
        AssignFlowField(FlowFieldManager.Instance.GetFlowField());
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
    }

    public void AssignFlowField(FlowField field)
    {
        flowField = field;
    }

    private void FixedUpdate()
    {
        if (flowField == null)
        {
            if (movement != null)
                movement.SetVelocity(Vector2.zero);
            else if (controller != null)
                controller.SetVelocity(Vector2.zero);
            return;
        }

        directionRefreshTimer -= Time.fixedDeltaTime;
        if (directionRefreshTimer <= 0f)
        {
            directionRefreshTimer = directionRefreshInterval;
            if (!flowField.TryGetFlowDirection(transform.position, out currentFlowDirection))
            {
                currentFlowDirection = Vector2.zero;
            }
        }

        Vector2 desiredVelocity = currentFlowDirection * moveSpeed;
        if (movement != null)
            movement.SetVelocity(desiredVelocity);
        else if (controller != null)
            controller.SetVelocity(desiredVelocity);
    }
}