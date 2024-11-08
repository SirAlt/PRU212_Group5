using UnityEngine;

[CreateAssetMenu]
public class HurtboxConfigs : ScriptableObject
{
    [Tooltip("Whether invincibility disables hit-checking entirely. Set to off to keep registering hits, but ignore (most of) their effects.")]
    public bool IntangibleInvincibility;

    [Tooltip("How to treat forced movement effects from attacks.")]
    public HitForceBehavior HitForceBehavior;

    [Tooltip("How to treat instant kill effects.")]
    public InstantKillBehavior InstantKillBehavior;
}

public enum HitForceBehavior
{
    Full,
    HorizontalOnly,
    Normalized,
    NormalizedHorizontalOnly,
    Ignore,
}

public enum InstantKillBehavior
{
    EnforceDeath,
    EnforceDeathWithHitForce,
    DealInfiniteDamage,
    DealMaxHealthAsDamage,
    DealCurrentHealthAsDamage,
    Ignore,
}
