using System.Collections;
using UnityEngine;

public class CrumblingPlatform : MonoBehaviour
{
    private const string IdleAnim = "Idle";
    private const string BreakAnim = "Break";
    private const string ReformAnim = "Reform";

    [SerializeField] private LayerMask interactableLayers;

    [SerializeField] private float shakeTime;
    [SerializeField] private float shakeAmplitude;
    [SerializeField] private float shakeInterval;
    [SerializeField] private float reformDelay;

    [SerializeField] private Collider2D _detector;
    [SerializeField] private Collider2D _collision;

    private SpriteRenderer _sprite;
    private Animator _anim;

    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _sprite.enabled = true;
        _anim.Play(IdleAnim, -1, 0f);
        _detector.enabled = true;
        _collision.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((interactableLayers & (1 << collision.gameObject.layer)) != 0)
        {
            StartCoroutine(nameof(Crumble));
        }
    }

    private IEnumerator Crumble()
    {
        _detector.enabled = false;

        // Warning shake
        var timer = 0f;
        var dir = 1;
        var originalPos = _sprite.transform.position;
        while (timer < shakeTime)
        {
            var amplitude = (1 - timer / shakeTime) * shakeAmplitude;
            dir *= -1;
            _sprite.transform.position = originalPos + new Vector3(amplitude * dir, 0f);
            yield return new WaitForSeconds(shakeInterval);
            timer += shakeInterval;
        }
        _sprite.transform.position = originalPos;

        // Fall
        _anim.Play(BreakAnim, -1, 0f);

        yield return new WaitForSeconds(reformDelay);

        // Reform
        _sprite.enabled = true;
        _anim.Play(ReformAnim, -1, 0f);
    }

    // AnimEvent: Break [2]
    public void FootingGone()
    {
        _collision.enabled = false;
    }

    // AnimEvent: Break [End]
    public void BreakFinished()
    {
        _sprite.enabled = false;
    }

    // AnimEvent: Reform [End]
    public void ReformationFinished()
    {
        _collision.enabled = true;
        _detector.enabled = true;
    }
}
