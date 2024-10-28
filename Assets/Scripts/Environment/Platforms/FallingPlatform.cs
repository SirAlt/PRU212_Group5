using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-5)]
public class FallingPlatform : MonoBehaviour, IMover
{
    private const float DestinationReachedOffset = 0.01f;

    [SerializeField] private LayerMask interactableLayers;

    [SerializeField] private float shakeTime;
    [SerializeField] private float shakeAmplitude;
    [SerializeField] private float shakeInterval;
    [SerializeField] private float fallSpeed;
    [SerializeField] private float fallDistance;
    [SerializeField] private float respawnDelay;
    [SerializeField] private float respawnTellTime;

    [SerializeField] private Collider2D _detector;
    [SerializeField] private Collider2D _collision;
    [SerializeField] private Transform _platform;

    private SpriteRenderer _sprite;
    private Animator _anim;
    private Rigidbody2D _rb;

    private Vector3 _positionBeforeFall;
    private Vector3 _fallDestination;
    private float _distanceFallen;
    private bool _isFalling;

    public Vector2 MoveVector { get; private set; }
    public bool CalculateOnly { get; set; }

    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
        _rb = _platform.GetComponent<Rigidbody2D>();

        _positionBeforeFall = _platform.transform.localPosition;
    }

    private void FixedUpdate()
    {
        if (!_isFalling) return;

        // Do falling here because we need to act before MovingPlatform capturer and PlayerController.
        if (_distanceFallen < fallDistance - DestinationReachedOffset)
        {
            // cf. WaypointMover for details on this ugly hack.
            var oldPos = (Vector2)_platform.transform.localPosition;
            var newPos = Vector2.MoveTowards(_platform.transform.localPosition, _fallDestination, fallSpeed * Time.fixedDeltaTime);
            MoveVector = newPos - oldPos;

            if (!CalculateOnly)
            {
                _rb.position += MoveVector;
                _platform.transform.position += (Vector3)MoveVector;
            }

            _distanceFallen += oldPos.y - newPos.y;
        }
        else
        {
            StartCoroutine(nameof(Respawn));
        }
    }

    private void OnEnable()
    {
        _isFalling = false;
        _distanceFallen = 0f;
        MoveVector = Vector2.zero;

        _platform.transform.localPosition = _positionBeforeFall;
        _sprite.enabled = true;
        _sprite.color = new Color(1f, 1f, 1f, 1f);
        _anim.speed = 1f;
        _collision.enabled = true;
        _detector.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((interactableLayers & (1 << collision.gameObject.layer)) != 0)
        {
            StartCoroutine(nameof(Fall));
        }
    }

    private IEnumerator Fall()
    {
        _detector.enabled = false;

        // Warning shake
        var timer = 0f;
        var dir = 1;
        var originalSpritePos = _sprite.transform.localPosition;
        while (timer < shakeTime)
        {
            var amplitude = (1 - timer / shakeTime) * shakeAmplitude;
            dir *= -1;
            _sprite.transform.localPosition = originalSpritePos + new Vector3(amplitude * dir, 0f, 0f);
            yield return new WaitForSeconds(shakeInterval);
            timer += shakeInterval;
        }
        _sprite.transform.localPosition = originalSpritePos;

        // Fall
        _isFalling = true;  // Cue FixedUpdate to handle falling.
        _distanceFallen = 0f;
        _positionBeforeFall = _platform.transform.localPosition;
        _fallDestination = _positionBeforeFall + new Vector3(0f, -1.0f * fallDistance);
        _anim.speed = 0f;
    }

    private IEnumerator Respawn()
    {
        // Stop falling
        _isFalling = false;
        _distanceFallen = 0f;
        MoveVector = Vector2.zero;

        // Disappear
        _sprite.enabled = false;
        _collision.enabled = false;
        yield return new WaitForSeconds(respawnDelay - respawnTellTime);

        // Telegraph respawn
        _platform.transform.localPosition = _positionBeforeFall;
        _sprite.enabled = true;
        _sprite.color = new Color(1f, 1f, 1f, 0.5f);
        yield return new WaitForSeconds(respawnTellTime);
        _sprite.color = new Color(1f, 1f, 1f, 1f);

        // Respawn
        _collision.enabled = true;
        _detector.enabled = true;
        _anim.speed = 1f;
    }
}
