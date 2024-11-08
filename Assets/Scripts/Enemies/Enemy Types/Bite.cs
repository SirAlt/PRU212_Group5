using UnityEngine;

public class Bite : Attack
{
    [SerializeField] private float lifestealRatio;

    private IHealable _host;

    protected override void Awake()
    {
        base.Awake();
        _host = transform.parent.GetComponentInChildren<IHealable>();
    }

    protected override void DealDamage(IHitReceptor target, Vector2 force)
    {
        base.DealDamage(target, force);
        _host?.Heal(damage * lifestealRatio);
    }
}
