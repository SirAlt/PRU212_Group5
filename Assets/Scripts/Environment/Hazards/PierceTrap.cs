using System.Collections;
using UnityEngine;

public class PierceTrap : Hazard, ITriggerable
{
    private const string IdleAnim = "Idle";
    private const string TriggeredAnim = "Triggered";
    private const string ExtendAnim = "Extend";
    private const string RetractAnim = "Retract";

    [SerializeField] private float tellTime;
    [SerializeField] private float stayTime;
    [SerializeField] private float rearmTime;
    [SerializeField] private Collider2D _detector;

    private Animator _anim;

    private State _state;

    private enum State
    {
        Armed,
        Triggering,
        Active,
        Retracting,
        Rearming,
    }

    protected override void Awake()
    {
        base.Awake();
        _anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _state = State.Armed;
        _detector.enabled = true;
    }

    protected override void Start()
    {
        //base.Start();
        SetActive(false);
    }

    bool ITriggerable.IsOn => _state != State.Armed && _state != State.Rearming;

    void ITriggerable.TriggerOn()
    {
        if (_state != State.Armed) return;
        StartCoroutine(nameof(Rickety));
    }

    void ITriggerable.TriggerOff()
    {
        if (_state == State.Triggering)
        {
            StopCoroutine(nameof(Rickety));
            _state = State.Armed;
            _detector.enabled = true;
            _anim.Play(IdleAnim, -1, 0f);
        }
        else if (_state == State.Active)
        {
            StopCoroutine(nameof(Shank));
            Retract();
        }
    }

    private IEnumerator Rickety()
    {
        _state = State.Triggering;

        _detector.enabled = false;
        _anim.Play(TriggeredAnim, -1, 0f);

        yield return new WaitForSeconds(tellTime);
        StartCoroutine(nameof(Shank));
    }

    private IEnumerator Shank()
    {
        _state = State.Active;

        // TODO: Multiple hitbox stages.
        SetActive(true);
        _anim.Play(ExtendAnim, -1, 0f);

        yield return new WaitForSeconds(stayTime);
        Retract();
    }

    private void Retract()
    {
        _state = State.Retracting;

        // TODO: Multiple hitbox stages.
        SetActive(false);
        _anim.Play(RetractAnim, -1, 0f);
    }

    // AnimEvent: Retract [End]
    public void OnFullyRetracted()
    {
        StartCoroutine(nameof(Rearm));
    }

    private IEnumerator Rearm()
    {
        _state = State.Rearming;

        yield return new WaitForSeconds(rearmTime);

        _state = State.Armed;
        _detector.enabled = true;
        //_anim.Play(IdleAnim, -1, 0f);     // [Retract] -> [Idle] has ExitTime already set in Animator
    }
}
