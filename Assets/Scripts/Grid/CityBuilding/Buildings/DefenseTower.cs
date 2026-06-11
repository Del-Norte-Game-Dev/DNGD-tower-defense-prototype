using UnityEngine;
using static BuildingData;

[RequireComponent(typeof(HealthComponent))]
public class DefenseTower : BuildingBehavior
{
    [Header("Tower Stats")]
    [SerializeField] private float range = 5f;
    [SerializeField] private float fireRate = 1f; //shots per second
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float retargetInterval = 0.25f;
    [SerializeField] private GameObject projectilePrefab; 

    private EnemyWaveManager enemyManager;
    [SerializeField] private LayerMask enemyLayerMask;

    private Collider2D[] retargetBuffer;
    private ContactFilter2D contactFilter;
    private float fireTimer = 0f;

    private GameObject currentTarget;
    private Transform towerPos;

    private HealthComponent healthComponent;


    public override void Init()
    {
        enemyManager = EnemyWaveManager.Instance;

        towerPos = gameObject.transform.Find("pivot");

        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(enemyLayerMask);
        contactFilter.useTriggers = true;

        retargetBuffer = new Collider2D[48];
        healthComponent = GetComponent<HealthComponent>();
        healthComponent.OnDead += OnDead;

        InvokeRepeating(nameof(Retarget), 0f, retargetInterval); // runs periodically to update target based on proximity to base
    }

    private void OnDead()
    {
        BuildManager.Instance.RemoveBuilding(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTarget == null) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= 1f / fireRate)
        {
            Shoot(currentTarget);
            fireTimer = 0f;
        }
    }

    void Retarget()
    {
        if (towerPos == null)
            return;

        int hitCount = Physics2D.OverlapCircle(towerPos.position, range, contactFilter, retargetBuffer);
        if (hitCount == 0)
        {
            currentTarget = null;
            return;
        }

        GameObject closest = null;
        float closestSqr = float.PositiveInfinity;
        Vector2 destination = enemyManager.defaultDestination;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = retargetBuffer[i];
            if (hit == null) continue;

            float sqrDist = ((Vector2)hit.transform.position - destination).sqrMagnitude;
            if (sqrDist < closestSqr)
            {
                closestSqr = sqrDist;
                closest = hit.gameObject;
            }
        }

        currentTarget = closest;
    }

    void Shoot(GameObject target)
    {
        if (target.TryGetComponent<HealthComponent>(out HealthComponent enemyHealth))
        {
            enemyHealth.TakeDamage(damage);
        }

        // Instantiate projectile and set its velocity towards the target
        TowerProjectile projectile = Instantiate(projectilePrefab, towerPos.position, Quaternion.LookRotation(target.transform.position - towerPos.position)).GetComponent<TowerProjectile>();
        if (projectile != null)
        {
            Vector2 direction = (target.transform.position - towerPos.position).normalized;
            projectile.Init(this, direction * bulletSpeed, Vector3.Distance(towerPos.position, target.transform.position) / bulletSpeed);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (towerPos == null)
        towerPos = transform.Find("pivot");

        if (towerPos == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(towerPos.position, range);
    }
}
