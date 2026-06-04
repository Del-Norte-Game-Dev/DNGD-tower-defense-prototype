using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.LowLevelPhysics2D.PhysicsShape;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float stopEpsilon = 0.01f;


    [SerializeField] private int health = 5;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 5;
    [SerializeField] private float attackRate = 5; //shots per second
    [SerializeField] private int attackDamage = 1;
    private GameObject currentTarget;
    [SerializeField] private LayerMask buildingLayer;
    private float fireTimer = 0f;

    private Rigidbody2D rb;

    private Vector2 currentVelocity;
    private Vector2 desiredVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating(nameof(Retarget), 0f, 0.15f);
    }

    private void FixedUpdate()
    {
        UpdateVelocity(Time.fixedDeltaTime);
        ApplyMovement();

        if (currentTarget == null) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= 1f / attackRate)
        {
            if (currentTarget.TryGetComponent<IBuilding>(out IBuilding building))
            {
                
                building.TakeDamage(attackDamage);
                fireTimer = 0f;
            }
            
        }
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

    void Retarget()
    {

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, buildingLayer);

        if (hits.Length == 0)
        {
            currentTarget = null;
            return;
        }

        Collider2D priorityTarget = null;
        Collider2D closestTarget = null;
        float closestPriorityDist = float.MaxValue;
        float closestDist = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);

            if (hit.CompareTag("Main Tower"))//change later
            {
                if (dist < closestPriorityDist)
                {
                    closestPriorityDist = dist;
                    priorityTarget = hit;
                }
            }
            else
            {
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestTarget = hit;
                }
            }
        }

        // attacks main tower first
        currentTarget = priorityTarget != null ? priorityTarget.gameObject : closestTarget.gameObject;
    }

    private GameObject GetBuildingInRange()
    {
        return Physics2D.OverlapCircle(transform.position, attackRange, buildingLayer).gameObject;
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