using System.Collections.Generic;
using UnityEngine;

public class DefenseTower : MonoBehaviour
{
    [Header("Tower Stats")]
    [SerializeField] private float range = 5f;
    [SerializeField] private float fireRate = 1f; //shots per second
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private int damage = 10;


    private EnemyWaveManager enemyManager;
    [SerializeField] private LayerMask enemyLayerMask;


    private List<Collider2D> hitBuffer = new List<Collider2D>();
    private ContactFilter2D contactFilter;
    private float fireTimer = 0f;

    private GameObject currentTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyManager = EnemyWaveManager.Instance;

        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(enemyLayerMask);
        contactFilter.useTriggers = true;

        InvokeRepeating(nameof(Retarget), 0f, 0.15f); // runs every 0.15 seconds to update target based on proximity to base
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
        
        // Fills hitBuffer with colliders in range — no new list allocated
        Physics2D.OverlapCircle(transform.position, range, contactFilter, hitBuffer);

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

        Debug.Log(hitBuffer);
    }

    void Shoot(GameObject target)
    {
        target.GetComponent<EntityController2D>()?.TakeDamage(10); // stop target movement
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
