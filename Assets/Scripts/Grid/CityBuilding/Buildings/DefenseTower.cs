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
    [SerializeField] private GameObject projectilePrefab; 


    private EnemyWaveManager enemyManager;
    [SerializeField] private LayerMask enemyLayerMask;


    private List<Collider2D> hitBuffer = new List<Collider2D>();
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

        damageable = true;
        currentHealth = maxHealth;

        InvokeRepeating(nameof(Retarget), 0f, 0.15f); // runs every 0.15 seconds to update target based on proximity to base
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
        // Fills hitBuffer with colliders in range — no new list allocated
        Physics2D.OverlapCircle(towerPos.position, range, contactFilter, hitBuffer);

        if (hitBuffer.Count == 0)
        {
            currentTarget = null;
            return;
        }

        GameObject closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider2D hit in hitBuffer)
        {
            if (hit == null) continue;

            float dist = Vector2.Distance(hit.transform.position, enemyManager.defaultDestination);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = hit.gameObject;
            }
        }
        currentTarget = closest;
    }

    void Shoot(GameObject target)
    {
        target.GetComponent<EntityController2D>()?.TakeDamage(10);

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
