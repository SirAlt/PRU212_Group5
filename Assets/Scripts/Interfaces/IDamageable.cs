using UnityEngine;

public interface IDamageable
{
    float CurrentHealth { get; }
    float MaxHealth { get; }

    void TakeDamage(float damage);
    void TakeDamage(float damage, Vector2 direction);

    void Die();
}
