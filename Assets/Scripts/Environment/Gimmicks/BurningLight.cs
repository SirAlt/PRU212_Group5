
using System;
using UnityEngine;

public class BurningLight : Attack
{
    public event Action EvilBurnt;

    protected override void DealDamage(IHitReceptor target, Vector2 force)
    {
        base.DealDamage(target, force);
        EvilBurnt?.Invoke();
    }
}
