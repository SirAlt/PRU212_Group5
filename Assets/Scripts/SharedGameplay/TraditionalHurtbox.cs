using UnityEngine;
using static IHitReceptor;

[RequireComponent(typeof(BoxCollider2D))]
public class TraditionalHurtbox : MonoBehaviour, IHitReceptor
{
    [SerializeField] private HurtboxConfigs _configs;

    public BoxCollider2D Collider { get; private set; }

    private IDamageable _host;
    private bool _hasInvincibility;
    private float _timeToEndInvincibility;

    private void Awake()
    {
        Collider = GetComponent<BoxCollider2D>();
        _host = transform.parent.GetComponentInChildren<IDamageable>();
    }

    private void FixedUpdate()
    {
        if (_hasInvincibility && Time.time >= _timeToEndInvincibility)
        {
            RemoveInvincibility();
        }
    }

    public void TakeHit(float damage, Vector2 force, HitAttributes hitAttributes)
    {
        if (_hasInvincibility && !hitAttributes.HasFlag(HitAttributes.PiercesInvincibility))
        {
            return;
        }

        switch (_configs.HitForceBehavior)
        {
            case HitForceBehavior.Full:
                break;
            case HitForceBehavior.HorizontalOnly:
                force.y = 0;
                break;
            case HitForceBehavior.Normalized:
                force = force.normalized;
                break;
            case HitForceBehavior.NormalizedHorizontalOnly:
                if (force.x != 0) force.x = Mathf.Sign(force.x);
                force.y = 0;
                break;
            case HitForceBehavior.Ignore:
                force = Vector2.zero;
                break;
        }

        if (hitAttributes.HasFlag(HitAttributes.InstantKill))
        {
            switch (_configs.InstantKillBehavior)
            {
                case InstantKillBehavior.EnforceDeath:
                    _host.Die();
                    return;
                case InstantKillBehavior.EnforceDeathWithHitForce:
                    _host.TakeDamage(0, force);
                    goto case InstantKillBehavior.EnforceDeath;

                case InstantKillBehavior.DealInfiniteDamage:
                    damage = MaxxDamage();
                    break;
                case InstantKillBehavior.DealMaxHealthAsDamage:
                    damage = _host.MaxHealth;
                    break;
                case InstantKillBehavior.DealCurrentHealthAsDamage:
                    damage = _host.CurrentHealth;
                    break;

                case InstantKillBehavior.Ignore:
                    break;
            }
        }

        _host.TakeDamage(damage, force);

        float MaxxDamage()
        {
            var dmg = 999f;
            for (int i = 3; i < 38; i++)
            {
                dmg = dmg + 9f * Mathf.Pow(10, i);
                if (dmg >= _host.MaxHealth * 10f) break;
            }
            return dmg;
        }
    }

    public void SetActive(bool active)
    {
        Collider.enabled = active;
    }

    public void GainInvincibility(float duration = -1.0f)
    {
        if (duration == 0f) return;

        if (!_hasInvincibility)
        {
            _hasInvincibility = true;
            if (_configs.IntangibleInvincibility) Collider.enabled = false;
        }

        if (duration < 0f)
        {
            _timeToEndInvincibility = Mathf.Infinity;
        }
        else if (Time.time + duration > _timeToEndInvincibility)
        {
            _timeToEndInvincibility = Time.time + duration;
        }
    }

    public void RemoveInvincibility()
    {
        if (_hasInvincibility)
        {
            _hasInvincibility = false;
            _timeToEndInvincibility = 0f;
            if (_configs.IntangibleInvincibility) Collider.enabled = true;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_configs == null)
        {
            Debug.LogWarning($"Please assign a(n) {nameof(HurtboxConfigs)} asset to the Configs slot on the {nameof(TraditionalHurtbox)} script of [ {gameObject.name} ]", gameObject);
        }
    }
#endif
}
