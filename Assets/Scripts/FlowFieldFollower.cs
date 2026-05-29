using UnityEngine;

public class FlowFieldFollower : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private EntityController2D controller;
    private FlowField flowField;

    private bool selected;

    private void Awake()
    {
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

    private void Update()
    {
        if (flowField == null)
        {
            controller.SetVelocity(Vector2.zero);
            return;
        }

        if (flowField.TryGetFlowDirection(transform.position, out Vector2 dir))
        {
            controller.SetVelocity(dir * moveSpeed);
        }
        else
        {
            controller.SetVelocity(Vector2.zero);
        }
    }
}