using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D), typeof(Light2D))]
public class Lantern : MonoBehaviour, ITriggerable
{
    private const float ScreenShakeIntensity = 1.0f;
    private const float ScreenShakeDuration = 2.0f;
    private const float BurnEffectLingerTime = 0.1f;
    private const string OnAnim = "On";
    private const string OffAnim = "Off";
    private const string BrokenAnim = "Broken";

    [SerializeField] private LayerMask triggeringLayers;
    [SerializeField] private float duration;
    [SerializeField] private float rearmTime;
    [SerializeField] private bool oneTime;
    [SerializeField] private bool startOn;
    [SerializeField] private ParticleSystem sparksPrefab;

    [Header("Scenario scripting")]
    [SerializeField] private bool reentrant;

    private SpriteRenderer _sprite;
    private Animator _anim;
    private Light2D _light;
    private Collider2D _hitbox;
    private ParticleSystem _sparksInstance;
    private Attack _damageZone;

    private State _state;

    private bool _burnEffectOn;
    private float _timeBurnEffectWasActivated;

    bool ITriggerable.IsOn => _state == State.On;

    void ITriggerable.TriggerOn() => LetThereBeLight();
    void ITriggerable.TriggerOff() => LightsOut();

    private enum State
    {
        Off,
        On,
        Recharge,
        Broken,
    }

    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
        _light = GetComponent<Light2D>();
        _hitbox = GetComponent<Collider2D>();
        _damageZone = GetComponentInChildren<Attack>();
    }

    private void OnEnable()
    {
        if (_state != State.Broken || reentrant)
        {
            _state = State.Off;
            _sprite.color = Color.white;
            _anim.Play(OffAnim, -1, 0f);
            _light.color = Color.yellow;
            _light.intensity = 0.1f;
            _hitbox.enabled = true;
        }

        if (startOn) LetThereBeLight();

        if (_damageZone is BurningLight burningLight)
        {
            burningLight.EvilBurnt += BurningRed;
        }
    }

    private void OnDisable()
    {
        _burnEffectOn = false;
        _timeBurnEffectWasActivated = 0f;

        _damageZone.SetActive(false);
        if (_damageZone is BurningLight burningLight)
        {
            burningLight.EvilBurnt -= BurningRed;
        }
    }

    private void Update()
    {
        if (_burnEffectOn && _timeBurnEffectWasActivated + BurnEffectLingerTime <= Time.time)
        {
            _burnEffectOn = false;
            _light.color = Color.yellow;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((triggeringLayers & (1 << collision.gameObject.layer)) != 0)
        {
            // If we're turned on in OnEnable() or through the ITriggerable interface, keep our hitbox active
            // and give feedback, but don't actually do anything beyond that.
            ScreenShake.Instance.Shake(ScreenShakeIntensity, ScreenShakeDuration);

            if (_state != State.Off) return;

            _hitbox.enabled = false;
            LetThereBeLight();
        }
    }

    private void LetThereBeLight()
    {
        if (_state != State.Off) return;
        _state = State.On;

        _anim.Play(OnAnim, -1, 0f);
        _light.intensity = 1.0f;
        _damageZone.SetActive(true);
        if (duration >= 0f)
            StartCoroutine(nameof(DyingLight));
    }

    private IEnumerator DyingLight()
    {
        yield return new WaitForSeconds(duration);

        _damageZone.SetActive(false);
        LightsOut();
    }

    private void LightsOut()
    {
        if (_state != State.On) return;
        _state = State.Recharge;

        _anim.Play(OffAnim, -1, 0f);
        _sprite.color = new Color(0.3f, 0.3f, 0.3f);
        _light.intensity = 0.01f;
        if (!oneTime)
            StartCoroutine(nameof(AGlimmerReignited));
        else
            LostHaven();
    }

    private IEnumerator AGlimmerReignited()
    {
        yield return new WaitForSeconds(rearmTime);

        _state = State.Off;
        _sprite.color = Color.white;
        _light.intensity = 0.1f;
        _hitbox.enabled = true;
    }

    private void LostHaven()
    {
        _state = State.Broken;
        _sprite.color = new Color(0.85f, 0.85f, 0.85f);
        _anim.Play(BrokenAnim, -1, 0f);
        ScreenShake.Instance.Shake(ScreenShakeIntensity, ScreenShakeDuration / 2f);
        _sparksInstance = Instantiate(sparksPrefab, transform.position, Quaternion.identity);
    }

    private void BurningRed()
    {
        _burnEffectOn = true;
        _light.color = Color.red;
        _timeBurnEffectWasActivated = Time.time;
    }
}
