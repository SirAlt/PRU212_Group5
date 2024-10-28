using UnityEngine;

public class TetherMover : MonoBehaviour, IMover
{
    private const float PerlinBiasCorrection = 0.05f;

    [SerializeField, Range(0f, 1f)] private float deceleration;

    [Header("Weights")]
    [SerializeField] private float tetherWeight;
    [SerializeField] private float strafeWeight;
    [SerializeField] private float altitudeWeight;

    [Header("Tethering")]
    [SerializeField] private float tetherDistance;
    [SerializeField] private float minTetherMoveSpeed;
    [SerializeField] private float maxTetherMoveSpeed;
    [SerializeField] private float deadzoneWidth;
    [SerializeField] private float innerHardZoneRadius;
    [SerializeField] private float outerHardZoneRadius;

    [Header("Strafe")]
    [SerializeField] private float minStrafeMoveSpeed;
    [SerializeField] private float maxStrafeMoveSpeed;
    [SerializeField] private float noiseScale;
    [SerializeField, Range(0f, 1f)] private float periodicFactor;
    [SerializeField, Range(0f, 1f)] private float smoothFactor;

    [Header("Altitude")]
    [SerializeField] private float distanceAboveTarget;
    [SerializeField] private float minAltChangeSpeed;
    [SerializeField] private float maxAltChangeSpeed;
    [SerializeField] private float altDeviationDeadzoneThreshold;
    [SerializeField] private float altDeviationHardzoneThreshold;

    private Rigidbody2D _rb;

    private Transform _target;
    private Vector2 _tetherVector;
    private Vector2 _tetherMove;
    private Vector2 _strafeMove;
    private Vector2 _altitudeMove;
    private float _strafeTime;

    private float _seed;
    private float _randomPhase;
    private Vector2 _randomStart;
    private Vector2 _randomDir;

    public Vector2 MoveVector { get; private set; }
    public bool CalculateOnly { get; set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _seed = Random.Range(0f, 100f);
        _randomPhase = _seed * 2f * Mathf.PI;
        _randomStart = new Vector2(_seed, _seed);
        _randomDir = new Vector2(Mathf.Cos(_randomPhase), Mathf.Sin(_randomPhase));
    }

    private void FixedUpdate()
    {
        if (_target == null) return;

        _tetherVector = _target.position - transform.position;

        Tether();
        Strafe();
        Altitude();
        ApplyMovement();
    }

    private void Tether()
    {
        var deviation = Vector3.Distance(_target.position, transform.position) - tetherDistance;

        if (Mathf.Abs(deviation) < deadzoneWidth)
        {
            _tetherMove = Vector2.Lerp(_tetherMove, Vector2.zero, deceleration);
        }
        else
        {
            float factor;
            if (deviation > 0)
                factor = deviation / (outerHardZoneRadius - tetherDistance);
            else
                factor = deviation / (innerHardZoneRadius - tetherDistance);

            var movespeed = Mathf.Lerp(minTetherMoveSpeed, maxTetherMoveSpeed, factor);

            // Find the ideal point to be at. Tether vector points towards target, so flip it to find the ideal position for self.
            var dest = _target.position - (Vector3)_tetherVector.normalized * tetherDistance;

            // cf. WaypointMover for details on this ugly hack.
            var oldPos = (Vector2)transform.position;
            var newPos = Vector2.MoveTowards(transform.position, dest, movespeed * Time.fixedDeltaTime);
            _tetherMove = newPos - oldPos;
        }
    }

    private void Strafe()
    {
        _strafeTime += Time.deltaTime;

        var sineFactor = Mathf.Sin(_strafeTime * noiseScale + _randomPhase);
        var noiseFactor = GetNoiseFactor();
        var factor = periodicFactor * sineFactor + (1 - periodicFactor) * noiseFactor;

        var clockwise = factor != 0f ? Mathf.Sign(factor) : 0f;
        var strafeDir = Vector2.Lerp(clockwise * Vector2.Perpendicular(_tetherVector).normalized, _strafeMove.normalized, smoothFactor);
        var speed = Mathf.Lerp(minStrafeMoveSpeed, maxStrafeMoveSpeed, Mathf.Abs(factor)) * Time.fixedDeltaTime;

        _strafeMove = strafeDir * speed;

        float GetNoiseFactor()
        {
            var v1 = _randomStart + _strafeTime * noiseScale * _randomDir;
            var v2 = new Vector2(_randomStart.x + _randomDir.x * _strafeTime * noiseScale * 1.5f, v1.y + 10.33f);
            float noise1 = (Mathf.PerlinNoise(v1.x, v1.y) - 0.5f) * 2f;
            float noise2 = (Mathf.PerlinNoise(v2.x, v2.y) - 0.5f) * 2f;
            return (noise1 + noise2 * 0.65f) / 1.65f + PerlinBiasCorrection;
        }
    }

    private void Altitude()
    {
        var targetAlt = _target.transform.position.y + distanceAboveTarget;
        var deviation = transform.position.y - targetAlt;

        if (Mathf.Abs(deviation) < altDeviationDeadzoneThreshold)
        {
            _altitudeMove = Vector2.Lerp(_altitudeMove, Vector2.zero, deceleration);
        }
        else
        {
            var factor = Mathf.Abs(deviation) / altDeviationHardzoneThreshold;
            var climbSpeed = Mathf.Lerp(minAltChangeSpeed, maxAltChangeSpeed, factor) * Time.fixedDeltaTime;
            _altitudeMove = new Vector2(0, -1.0f * Mathf.Sign(deviation) * climbSpeed);
        }
    }

    private void ApplyMovement()
    {
        MoveVector = (tetherWeight * _tetherMove) + (strafeWeight * _strafeMove) + (altitudeWeight * _altitudeMove);
        if (!CalculateOnly)
        {
            _rb.position += MoveVector;
            transform.position += (Vector3)MoveVector;
        }
    }

    public void SetTarget(Transform target) => _target = target;
    public void UnsetTarget() => _target = null;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (innerHardZoneRadius < 0f)
        {
            Debug.LogWarning($"Negative inner hard zone radius (distance for maximum \"get-away\" speed) for tethering movement on {gameObject.name}.", gameObject);
        }
        if (Mathf.Abs(tetherWeight + strafeWeight + altitudeWeight - 1f) > 1e-5f)
        {
            Debug.LogWarning($"Weights of tethering movement on {gameObject.name} do not add up to 1.", gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)_tetherVector);

        Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(transform.position, tetherDistance);
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)_tetherMove * 10f);

        Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, innerHardZoneRadius);
        //Gizmos.DrawWireSphere(transform.position, outerHardZoneRadius);
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)_strafeMove * 10f);

        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, tetherDistance - deadzoneWidth);
        //Gizmos.DrawWireSphere(transform.position, tetherDistance + deadzoneWidth);
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)_altitudeMove * 10f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)MoveVector * 10f);
    }
#endif
}
