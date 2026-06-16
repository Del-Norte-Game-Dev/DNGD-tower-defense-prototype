using UnityEngine;

public class FlowFieldFollower : MonoBehaviour
{
    [SerializeField] private float directionRefreshInterval = 0.12f;

    private EntityRbMovement2D movement;
    private FlowField flowField;

    private Vector2 currentFlowDirection;
    private float directionRefreshTimer;

    private void Awake()
    {
        movement = GetComponent<EntityRbMovement2D>();
    }

    private void Start()
    {
        AssignFlowField(FlowFieldManager.Instance.GetFlowField());
    }

    public void AssignFlowField(FlowField field)
    {
        flowField = field;
    }

    private void FixedUpdate()
    {
        if (flowField == null)
            movement.SetDirection(Vector2.zero);

        directionRefreshTimer -= Time.fixedDeltaTime;
        if (directionRefreshTimer <= 0f)
        {
            directionRefreshTimer = directionRefreshInterval;
            if (!flowField.TryGetFlowDirection(transform.position, out currentFlowDirection))
            {
                currentFlowDirection = Vector2.zero;
            }
        }

        Vector2 desiredDir = currentFlowDirection;
        if (movement != null)
            movement.SetDirection(desiredDir);
    }
}