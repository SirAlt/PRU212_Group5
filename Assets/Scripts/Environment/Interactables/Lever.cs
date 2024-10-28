using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Lever : MonoBehaviour, ITriggerable
{
    private const string OnAnim = "On";
    private const string OffAnim = "Off";
    private const string TurnOnAnim = "TurnOn";
    private const string TurnOffAnim = "TurnOff";

    [SerializeField] private LayerMask triggerLayers;
    [SerializeField] private List<GameObject> controlTargets = new();
    [SerializeField] private bool startOn;
    [SerializeField] private bool oneTime;
    [SerializeField] private float autoResetTime;

    [Header("VFX")]
    [SerializeField] private float screenShakeIntensity;
    [SerializeField] private float screenShakeDuration;

    [Header("Scenario scripting")]
    [SerializeField] private bool reentrant;

    private SpriteRenderer _sprite;
    private Animator _anim;
    private Collider2D _hitbox;
    private readonly List<ITriggerable> _targets = new();
    private bool _isOn;
    private bool _crankQueued;

    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();

        _hitbox = GetComponent<Collider2D>();
        _hitbox.isTrigger = true;
        _hitbox.enabled = true;

        foreach (var controlTarget in controlTargets)
        {
            _targets.Add(controlTarget.GetComponentInChildren<ITriggerable>());
        }
    }

    private void OnEnable()
    {
        _isOn = startOn;
        _crankQueued = false;
        _sprite.color = Color.white;
        _anim.Play(startOn ? OnAnim : OffAnim, -1, 0f);
        if (!oneTime || reentrant) _hitbox.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((triggerLayers & (1 << collision.gameObject.layer)) != 0)
        {
            _hitbox.enabled = false;
            ScreenShake.Instance.Shake(screenShakeIntensity, screenShakeDuration);
            Crank();
        }
    }

    private void Crank()
    {
        if (!_isOn)
            CrankOn();
        else
            CrankOff();

        if (!_crankQueued && autoResetTime > 0f)
        {
            _crankQueued = true;
            Invoke(nameof(Crank), autoResetTime);
        }
        else if (_crankQueued)
        {
            _crankQueued = false;
        }
    }

    private void CrankOn()
    {
        if (_isOn) return;
        _isOn = true;
        foreach (var target in _targets) target?.TriggerOn();
        _anim.Play(TurnOnAnim, -1, 0f);
    }

    private void CrankOff()
    {
        if (!_isOn) return;
        _isOn = false;
        foreach (var target in _targets) target?.TriggerOff();
        _anim.Play(TurnOffAnim, -1, 0f);
    }

    // AnimEvent: TurnOn [End] & TurnOff [End]
    private void TurnCompleted()
    {
        if (_isOn) _anim.Play(OnAnim, -1, 0f);
        else _anim.Play(OffAnim, -1, 0f);

        if (!_crankQueued)
        {
            if (!oneTime)
                _hitbox.enabled = true;
            else
                _sprite.color = new Color(0.3f, 0.3f, 0.3f);
        }
    }

    public bool IsOn => _isOn;

    void ITriggerable.TriggerOn() => CrankOn();
    void ITriggerable.TriggerOff() => CrankOff();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (controlTargets != null && controlTargets.Count == 0)
        {
            Debug.LogWarning($"No control target set for [ {gameObject.name} ].", gameObject);
        }
        else if (controlTargets != null)
        {
            for (var i = 0; i < controlTargets.Count; i++)
            {
                if (controlTargets[i].GetComponentInChildren<ITriggerable>() == null)
                    Debug.LogWarning($"Control target at index {i} of [ {gameObject.name} ] does not have a(n) {nameof(ITriggerable)} component.", gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (var controlTarget in controlTargets)
        {
            Gizmos.DrawLine(transform.position, controlTarget.transform.position);
        }
    }
#endif
}
