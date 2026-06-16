using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityRbMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float stopEpsilon = 0.01f;

    private Vector2 currentVelocity;
    private Vector2 desiredVelocity;

    private Rigidbody2D rb;

    private float sqrStopEpsilon => stopEpsilon * stopEpsilon;

    public Vector2 CurrentVelocity => currentVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 dir)
    {
        dir.Normalize();
        Vector2 velocity = dir * moveSpeed;
        desiredVelocity = Vector2.ClampMagnitude(velocity, maxSpeed);

        if (desiredVelocity.sqrMagnitude < sqrStopEpsilon)
        {
            desiredVelocity = Vector2.zero;
        }
    }

    public void Stop()
    {
        currentVelocity = Vector2.zero;
        desiredVelocity = Vector2.zero;

        rb.linearVelocity = Vector2.zero; // optional safety
    }

    private void FixedUpdate()
    {
        UpdateVelocity(Time.fixedDeltaTime);
        ApplyMovement(Time.fixedDeltaTime);
    }

    private void UpdateVelocity(float deltaTime)
    {
        if (currentVelocity.sqrMagnitude < sqrStopEpsilon &&
            desiredVelocity.sqrMagnitude < sqrStopEpsilon)
        {
            return;
        }

        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            desiredVelocity,
            acceleration * deltaTime
        );
    }

    private void ApplyMovement(float deltaTime)
    {
        if (currentVelocity.sqrMagnitude < sqrStopEpsilon)
            return;

        Vector2 newPosition = rb.position + currentVelocity * deltaTime;
        rb.MovePosition(newPosition);
        ApplyRotatation();
    }

    private void ApplyRotatation()
    {
        if (currentVelocity.sqrMagnitude < sqrStopEpsilon)
        return;

        float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
    }
}