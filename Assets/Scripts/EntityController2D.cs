using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float stopEpsilon = 0.01f;


    [SerializeField] private int health = 5;
    private Rigidbody2D rb;

    private Vector2 currentVelocity;
    private Vector2 desiredVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        UpdateVelocity(Time.fixedDeltaTime);
        ApplyMovement();
    }

    public void SetVelocity(Vector2 velocity)
    {
        desiredVelocity = Vector2.ClampMagnitude(velocity, maxSpeed);

        if (desiredVelocity.sqrMagnitude < stopEpsilon * stopEpsilon)
        {
            desiredVelocity = Vector2.zero;
        }
    }

    private void UpdateVelocity(float deltaTime)
    {
        //smooth
        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            desiredVelocity,
            acceleration * deltaTime
        );
    }

    private void ApplyMovement()
    {
        rb.linearVelocity = currentVelocity;
    }

    public Vector2 GetVelocity()
    {
        return currentVelocity;
    }

    public void Stop()
    {
        currentVelocity = Vector2.zero;
        desiredVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            EnemyWaveManager.Instance.EnemyDefeated(this.gameObject);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        //current velocity (green)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)currentVelocity);

        //desired velocity (blue)
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)desiredVelocity);
    }
#endif
}