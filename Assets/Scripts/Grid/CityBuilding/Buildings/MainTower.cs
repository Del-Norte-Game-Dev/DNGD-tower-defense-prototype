using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.LowLevelPhysics2D.PhysicsShape;


[RequireComponent(typeof(HealthComponent))]
public class MainTower : BuildingBehavior
{
    private HealthComponent healthComponent;

    private Slider healthBarUI;


    public override void Init()
    {
        healthBarUI = GameObject.Find("Head Tower Health Bar").GetComponent<Slider>();
        healthComponent = GetComponent<HealthComponent>();
        healthComponent.OnDead += OnDead;
        healthComponent.OnHealthChanged += (a, b, c) => UpdateUIHealthBar();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void UpdateUIHealthBar()
    {
        healthBarUI.value = healthComponent.HealthPercent;
    }

    private void OnDead()
    {
        Debug.Log("Main Tower Destroyed! Game Over!");
    }
}
