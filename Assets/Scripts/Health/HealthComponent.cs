using UnityEngine;
using System;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private float CurrentHealth = 10f;
    [SerializeField] private float MaxHealth = 10f;

    public float HealthPercent => MaxHealth <= 0 ? 0 : CurrentHealth / MaxHealth;

    ///<param name="damage"></param>
    ///<param name="current health"></param>
    ///<param name="percent health"></param>
    public event Action<float, float, float> OnHealthChanged; // change to a struct
    public event Action OnDead;

    void Awake()
    {
        OnHealthChanged?.Invoke(0f, CurrentHealth, HealthPercent);
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || CurrentHealth <= 0f) return;

        CurrentHealth -= damage;
        CurrentHealth = Math.Max(CurrentHealth, 0);

        OnHealthChanged?.Invoke(damage, CurrentHealth, HealthPercent);

        if (CurrentHealth == 0)
            OnDead?.Invoke();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || CurrentHealth <= 0f) return;

        CurrentHealth += amount;
        CurrentHealth = Math.Min(CurrentHealth, MaxHealth);

        OnHealthChanged?.Invoke(-amount, CurrentHealth, HealthPercent);
    }
}
