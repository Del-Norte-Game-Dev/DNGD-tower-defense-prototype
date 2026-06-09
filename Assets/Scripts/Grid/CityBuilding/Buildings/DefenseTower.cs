using System;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using static BuildingData;

public class DefenseTower : MonoBehaviour, IBuilding
{
    [Header("Tower Stats")]
    [SerializeField] private float range = 5f;
    [SerializeField] private float fireRate = 1f; //shots per second
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float retargetInterval = 0.25f;
    [SerializeField] private GameObject projectilePrefab; 


    private EnemyWaveManager enemyManager;
    [SerializeField] private LayerMask enemyLayerMask;


    private Collider2D[] retargetBuffer;
    private ContactFilter2D contactFilter;
    private float fireTimer = 0f;

    private GameObject currentTarget;
    private Transform towerPos;

    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private bool damageable = false;


    public void Init()
    {
        enemyManager = EnemyWaveManager.Instance;

        towerPos = gameObject.transform.Find("pivot");

        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(enemyLayerMask);
        contactFilter.useTriggers = true;

        retargetBuffer = new Collider2D[48];
        damageable = true;
        currentHealth = maxHealth;

        InvokeRepeating(nameof(Retarget), 0f, retargetInterval); // runs periodically to update target based on proximity to base
    }

    public void TakeDamage(int damage)
    {
        if (damageable)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                BuildManager.Instance.RemoveBuilding(transform.position);
            }
        }
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
        if (target.TryGetComponent<EntityController2D>(out EntityController2D enemy))
        {
            enemy.TakeDamage(damage);
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(towerPos.position, range);
    }
}
