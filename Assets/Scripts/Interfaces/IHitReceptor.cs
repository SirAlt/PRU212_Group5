using System;
using UnityEngine;

public interface IHitReceptor
{
    void TakeHit(float damage, Vector2 force, HitAttributes hitAttributes = 0);

    void SetActive(bool active);

    [Flags]
    public enum HitAttributes
    {
        None = 0,
        InstantKill = 1,
        PiercesInvincibility = 2,
    }
}
