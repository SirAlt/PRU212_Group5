using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public class Flamethrower : Hazard
{
    [SerializeField] private float _activeDuration;
    [SerializeField] private float _dormantDuration;

    private SpriteRenderer _sprite;
    private Animator _animator;
    private Light2D _light;

    private float _timer;
    private bool _active;

    protected override void Awake()
    {
        base.Awake();

        _sprite = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _light = GetComponent<Light2D>();
    }

    protected override void Start()
    {
        base.Start();
        if (_activeDuration == 0)
            SetFlame(false);
        else
            SetFlame(true);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if ((!_active && _activeDuration == 0)
            || (_active && _dormantDuration == 0))
        {
            return;
        }

        _timer -= Time.fixedDeltaTime;
        if (_timer <= 0)
        {
            if (_active)
                SetFlame(false);
            else
                SetFlame(true);
        }
    }

    private void SetFlame(bool on)
    {
        _active = on;
        _timer = on ? _activeDuration : _dormantDuration;
        _sprite.enabled = on;
        _animator.enabled = on;
        if (_light) _light.enabled = on;
        SetActive(on);
    }
}
