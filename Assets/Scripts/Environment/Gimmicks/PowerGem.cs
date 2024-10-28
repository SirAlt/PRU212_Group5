using System.Collections;
using UnityEngine;

public class PowerGem : MonoBehaviour
{
    private const string GleamAnimationName = "Gleam";

    [SerializeField] private float minTimeBetweenGleams = 1.0f;
    [SerializeField] private float maxTimeBetweenGleams = 3.0f;

    [SerializeField] private LayerMask playerAttackLayers;
    [SerializeField] private float hitStopDuration = 0.1f;
    [SerializeField] private float respawnDelay;

    private SpriteRenderer _sprite;
    private Animator _animator;
    private Collider2D _hitbox;

    private float _timer;

    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _hitbox = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        _sprite.enabled = true;
        _animator.enabled = true;
        _hitbox.enabled = true;
    }

    private void Update()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) return;
        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            _animator.Play(GleamAnimationName, -1, 0f);
            _timer = Random.Range(minTimeBetweenGleams, maxTimeBetweenGleams);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player attacks are nested 2 levels down.
        if ((playerAttackLayers & (1 << collision.gameObject.layer)) != 0
            && collision.transform.parent.parent.TryGetComponent<PlayerController>(out var player))
        {
            HitStop.Instance.Stop(hitStopDuration);
            //player.AirJumpCharges = System.Math.Min(player.AirJumpCharges + 1, player.Abilities.AirJumpCharges);
            //player.AirDashCharges = System.Math.Min(player.AirDashCharges + 1, player.Abilities.AirDashCharges);
            player.AirJumpCharges = 1;
            StartCoroutine(nameof(Disappear));
        }
    }

    private IEnumerator Disappear()
    {
        _sprite.enabled = false;
        _animator.enabled = false;
        _hitbox.enabled = false;
        yield return new WaitForSeconds(respawnDelay);

        _sprite.enabled = true;
        _animator.enabled = true;
        _hitbox.enabled = true;
    }
}
