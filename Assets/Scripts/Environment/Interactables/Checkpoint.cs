using UnityEngine;
using static Constants;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour, ITriggerable
{
    private CheckpointSystem _checkpointSys;

    private SpriteRenderer _sprite;
    private Animator _anim;

    private bool _isActiveCheckpoint;

    private void Awake()
    {
        _checkpointSys = GetComponentInParent<CheckpointSystem>();

        _sprite = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.parent.CompareTag(PlayerTag))
        {
            if (!_isActiveCheckpoint)
            {
                _checkpointSys.SetLastCheckpoint(this);
            }
            if (collision.transform.parent.TryGetComponent<IHealable>(out var target))
            {
                target.Heal(target.MaxHealth);
            }
        }
    }

    public void Activate()
    {
        _isActiveCheckpoint = true;
        _sprite.color = Color.white;
        _anim.speed = 1.0f;
    }

    public void Deactivate()
    {
        _isActiveCheckpoint = false;
        _sprite.color = new Color(0.4f, 0.4f, 0.4f);
    }

    public void OnAnimationReachedLowestPoint()
    {
        if (!_isActiveCheckpoint)
        {
            _anim.speed = 0f;
        }
    }

    bool ITriggerable.IsOn => _isActiveCheckpoint;

    void ITriggerable.TriggerOn()
    {
        if (!_isActiveCheckpoint) _checkpointSys.SetLastCheckpoint(this);
    }

    void ITriggerable.TriggerOff()
    {
        _checkpointSys.ResetCheckpoint();
    }
}
