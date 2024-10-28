public interface IHealable
{
    float CurrentHealth { get; }
    float MaxHealth { get; }

    bool Heal(float amount);
}
