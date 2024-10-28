using System.Collections.Generic;
using UnityEngine;

public class DarkBall : Attack
{
    [Header("Special")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime;
    [SerializeField] private float turnRate;
    [SerializeField] private float homingDelay;
    [SerializeField] private bool supercharged;
    [SerializeField] private float superchargeScaleMultiplier = 2.0f;
    [SerializeField] private float superchargeDamageMultiplier = 2.0f;
    [SerializeField] private LayerMask projectileDestroyingLayers;
    [SerializeField] private LayerMask terrainLayers;

    private SpriteRenderer _sprite;
    private Animator _anim;
    private ContactFilter2D _terrainFilter;
    private readonly List<Collider2D> _overlaps = new();

    private Transform _target;
    private float _lifetimeTimer;
    private bool _exploding;

    private Vector3 _originalScale;
    private float _originalDamage;

    protected override void Awake()
    {
        base.Awake();
        _sprite = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();

        _terrainFilter = new ContactFilter2D();
        _terrainFilter.SetLayerMask(terrainLayers);
    }

    private void OnDisable()
    {
        _exploding = false;
        _lifetimeTimer = 0f;
        _anim.ResetTrigger("explode");

        if (supercharged)
        {
            transform.localScale = _originalScale;
            damage = _originalDamage;
        }

        if (gameObject.activeSelf) gameObject.SetActive(false);
    }

    protected override void Start()
    {
        base.Start();
        _originalScale = transform.localScale;
        _originalDamage = damage;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (_exploding) return;

        if (Hitbox.OverlapCollider(_terrainFilter, _overlaps) > 0)
        {
            if (supercharged) ExplodeSuperchargedly();
            else Explode();
            return;
        }

        _lifetimeTimer -= Time.fixedDeltaTime;
        if (_lifetimeTimer <= 0)
        {
            Explode();
            return;
        }

        if (_target != null && lifetime - _lifetimeTimer > homingDelay)
        {
            RotateTowards((_target.position - transform.position).normalized, turnRate);
        }
        transform.Translate(speed * Time.fixedDeltaTime * Vector2.right);
    }

    public void Launch(Transform target, Vector2 direction)
    {
        gameObject.SetActive(true);
        _exploding = false;
        _target = target;
        _lifetimeTimer = lifetime;
        SetActive(true);

        if (direction != Vector2.zero) StraightAhead(direction);
        else HomeIn();
    }

    private void StraightAhead(Vector2 direction)
    {
        RotateTowards(direction);
    }

    private void HomeIn()
    {
        var directionToTarget = (_target.position - transform.position).normalized;
        RotateTowards(directionToTarget);
    }

    private void RotateTowards(Vector3 targetDirection, float maxDegreesDelta = 360f)
    {
        var v = transform.right;
        var w = targetDirection.normalized;
        var rot = Mathf.Atan2(w.y * v.x - w.x * v.y, w.x * v.x + w.y * v.y) * Mathf.Rad2Deg;
        rot = Mathf.Clamp(rot, -maxDegreesDelta, maxDegreesDelta);
        transform.Rotate(0f, 0f, rot);

        var zEuler = transform.eulerAngles.z % 360;
        if (zEuler > 90 && zEuler < 270) _sprite.flipY = true;
        else _sprite.flipY = false;
    }

    protected override void TryHitTarget(Collider2D collision)
    {
        if (!_exploding
            && (projectileDestroyingLayers & (1 << collision.gameObject.layer)) != 0)
        {
            if (supercharged) ExplodeSuperchargedly();
            else Explode();
        }
        base.TryHitTarget(collision);
    }

    protected override void DealDamage(IHitReceptor target, Vector2 force)
    {
        if (!_exploding) Explode();
        base.DealDamage(target, force);
    }

    private void Explode()
    {
        _exploding = true;
        _anim.SetTrigger("explode");
    }

    private void ExplodeSuperchargedly()
    {
        transform.localScale *= superchargeScaleMultiplier;
        damage *= superchargeDamageMultiplier;
        Explode();
    }

    //AnimEvent: Explode [5]
    public void DisableHitbox()
    {
        SetActive(false);
    }

    // AnimEvent: Explode [End]
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
