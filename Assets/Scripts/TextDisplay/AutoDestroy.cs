using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float LifeSeconds = 2f;
    private float timer = 0f;
    private float speed = 1f;

    void Awake()
    {
        speed += Random.Range(0.1f, 0.1f);
    }

    void Update()
    {
        timer += Time.deltaTime;
        transform.position += Vector3.up * Time.deltaTime * speed;
        if (timer >= LifeSeconds)
        {
            Destroy(this.gameObject);
        }
    }
}
