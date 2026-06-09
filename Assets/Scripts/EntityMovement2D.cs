using UnityEngine;

public class EntityMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float stopEpsilon = 0.01f;

    private Vector2 currentVelocity;
    private Vector2 desiredVelocity;

    private float sqrStopEpsilon => stopEpsilon * stopEpsilon;

    public Vector2 CurrentVelocity => currentVelocity;

    public void SetVelocity(Vector2 velocity)
    {
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
    }

    private void FixedUpdate()
    {
        UpdateVelocity(Time.fixedDeltaTime);
        ApplyMovement();
    }

    private void UpdateVelocity(float deltaTime)
    {
        if (currentVelocity.sqrMagnitude < sqrStopEpsilon && desiredVelocity.sqrMagnitude < sqrStopEpsilon)
        {
            return;
        }

        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            desiredVelocity,
            acceleration * deltaTime
        );
    }

    private void ApplyMovement()
    {
        if (currentVelocity.sqrMagnitude < sqrStopEpsilon)
            return;

        transform.position += (Vector3)(currentVelocity * Time.fixedDeltaTime);
    }
}
