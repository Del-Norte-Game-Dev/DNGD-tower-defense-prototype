using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private HealthComponent healthComponent;
    MeshRenderer meshRenderer;
    MaterialPropertyBlock block;

    void Awake()
    {
        healthComponent = GetComponentInParent<HealthComponent>();
        meshRenderer = GetComponent<MeshRenderer>();
        block = new MaterialPropertyBlock();

        healthComponent.OnHealthChanged += UpdateHealthBar;
    }

    void UpdateHealthBar(float damage, float currentHealth, float percent)
    {
        meshRenderer.GetPropertyBlock(block);
        block.SetFloat("_PercentHealth", percent);
        meshRenderer.SetPropertyBlock(block);
    }
}
