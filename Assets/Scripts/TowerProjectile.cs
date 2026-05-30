using System.Collections;
using UnityEngine;

public class TowerProjectile : MonoBehaviour
{
    private DefenseTower tower;

    public void Init(DefenseTower tower, Vector2 velocity, float time)
    {
        this.tower = tower;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }

        StartCoroutine(DestroyProj(time));
    }

    IEnumerator DestroyProj(float time)
    {
        
        yield return new WaitForSecondsRealtime(time); // wait a frame to ensure damage is applied before destroying
        Destroy(this.gameObject);
    }

}