using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Constants;

public class PlayerFX : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] private float minScreenShake;
    [SerializeField] private float maxScreenShake;
    [SerializeField] private float screenShakeDuration;

    private PlayerController _player;
    private SpriteRenderer _sprite;
    private Animator _animator;

    private Slider _healthBar;

    private void Awake()
    {
        _player = transform.parent.GetComponent<PlayerController>();
        _sprite = transform.parent.GetComponent<SpriteRenderer>();
        _animator = transform.parent.GetComponent<Animator>();
        _healthBar = GameObject.FindGameObjectWithTag(HealthBarTag).GetComponent<Slider>();
    }

    private void OnEnable()
    {
        CharacterEvents.characterDamaged += ShakeScreenOnDamage;
    }

    private void OnDisable()
    {
        CharacterEvents.characterDamaged -= ShakeScreenOnDamage;
    }

    private void ShakeScreenOnDamage(GameObject obj, float value)
    {
        // Bleh! Why can't we just get an event just for the player?!
        if (obj != _player.gameObject) return;
        var intensity = Mathf.Lerp(minScreenShake, maxScreenShake, value / _player.MaxHealth);
        ScreenShake.Instance.Shake(intensity, screenShakeDuration);
    }

    public void StartFlicker(float duration)
    {
        StopFlicker();
        StartCoroutine(nameof(Flicker));
        Invoke(nameof(StopFlicker), duration);
    }

    private IEnumerator Flicker()
    {
        while (true)
        {
            _sprite.enabled = !_sprite.enabled;
            yield return new WaitForSeconds(_player.Stats.InvincibilityFlickerInterval);
        }
    }

    public void StopFlicker()
    {
        StopCoroutine(nameof(Flicker));
        _sprite.enabled = true;
    }

    public void UpdateHealthBar()
    {
        _healthBar.value = _player.CurrentHealth / _player.MaxHealth;
    }

    public void StopAllEffects()
    {
        StopFlicker();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
    }
#endif
}
