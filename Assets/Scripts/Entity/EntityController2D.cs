using System;
using UnityEngine;

public class EntityController2D : MonoBehaviour
{
    public static int ENTITY_COUNT;

    private EntityRbMovement2D movement;

    [SerializeField] private float maxHealth = 5;
    private HealthComponent health;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 5;
    [SerializeField] private float attackRate = 5; //shots per second
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float retargetInterval = 0.25f;
    private GameObject currentTarget;
    [SerializeField] private LayerMask buildingLayer;
    private float fireTimer = 0f;

    private readonly Collider2D[] retargetBuffer = new Collider2D[64];
    private ContactFilter2D buildingContactFilter;
    private float retargetTimer;
    private float attackRangeSqr => attackRange * attackRange;

    private void Awake()
    {
        movement = GetComponent<EntityRbMovement2D>();

        buildingContactFilter = new ContactFilter2D();
        buildingContactFilter.SetLayerMask(buildingLayer);
        buildingContactFilter.useTriggers = true;

        retargetTimer = UnityEngine.Random.Range(0f, retargetInterval);

        health = GetComponent<HealthComponent>();
        health.OnDead += OnDead;


    }

    void OnEnable()
    {
        ENTITY_COUNT++;
    }

    private void OnDisable()
    {
        ENTITY_COUNT--;
    }

    private void FixedUpdate()
    {
        retargetTimer -= Time.fixedDeltaTime;
        if (retargetTimer <= 0f)
        {
            retargetTimer += retargetInterval;
            if (!IsCurrentTargetValid())
                Retarget();
        }

        if (currentTarget == null) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= 1f / attackRate)
        {
            if (currentTarget.TryGetComponent<HealthComponent>(out HealthComponent building))
            {
                building.TakeDamage(attackDamage);
                fireTimer = 0f;
            }
        }
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (movement != null)
        {
            movement.SetVelocity(velocity);
        }
    }

    public Vector2 GetVelocity()
    {
        return movement != null ? movement.CurrentVelocity : Vector2.zero;
    }

    public void Stop()
    {
        movement?.Stop();
    }

    private void OnDead()
    {
        EnemyWaveManager.Instance.EnemyDefeated(this.gameObject);
    }

    void Retarget()
    {
        int hitCount = Physics2D.OverlapCircle(transform.position, attackRange, buildingContactFilter, retargetBuffer);

        if (hitCount == 0)
        {
            currentTarget = null;
            return;
        }

        Collider2D priorityTarget = null;
        Collider2D closestTarget = null;
        float closestPrioritySqr = float.MaxValue;
        float closestSqr = float.MaxValue;
        Vector2 position = transform.position;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = retargetBuffer[i];
            if (hit == null) continue;

            float sqrDist = (position - (Vector2)hit.transform.position).sqrMagnitude;
            if (hit.CompareTag("Main Tower")) //change later
            {
                if (sqrDist < closestPrioritySqr)
                {
                    closestPrioritySqr = sqrDist;
                    priorityTarget = hit;
                }
            }
            else if (sqrDist < closestSqr)
            {
                closestSqr = sqrDist;
                closestTarget = hit;
            }
        }

        currentTarget = priorityTarget != null ? priorityTarget.gameObject : closestTarget?.gameObject;
    }

    private bool IsCurrentTargetValid()
    {
        if (currentTarget == null) return false;
        if (!currentTarget.activeInHierarchy) return false;

        var targetPosition = (Vector2)currentTarget.transform.position;
        if (((Vector2)transform.position - targetPosition).sqrMagnitude > attackRangeSqr) return false;

        return currentTarget.TryGetComponent<IBuilding>(out _);
    }

    private GameObject GetBuildingInRange()
    {
        return Physics2D.OverlapCircle(transform.position, attackRange, buildingLayer)?.gameObject;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (movement == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)movement.CurrentVelocity);
    }
#endif
}