using Pathfinding;
using System;
using UnityEngine;

public class ChaseMover : MonoBehaviour, IMover
{
    [SerializeField] private float acceleration;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float slowdownDistance;
    [SerializeField] private float stoppingDistance;
    [SerializeField, Range(0f, 1f)] private float decelerationRate = 0.1f;

    [Header("Pathfinding")]
    [SerializeField] private bool usePathfinding;
    [SerializeField] private LayerMask terrainLayers;
    [SerializeField, Range(0f, 1f)] private float smoothingFactor;

    private Rigidbody2D _rb;
    [SerializeField] private Transform _target;
    private float _speed;

    private AILerp _aiLerp;

    public Vector2 MoveVector { get; private set; }
    public bool CalculateOnly { get; set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (usePathfinding)
        {
            _aiLerp = GetComponent<AILerp>();
        }
    }

    private void Start()
    {
        if (usePathfinding)
        {
            _aiLerp.canMove = false;
            _aiLerp.enableRotation = false;
            _aiLerp.updateRotation = false;
            _aiLerp.orientation = OrientationMode.YAxisForward;
            _aiLerp.interpolatePathSwitches = false;
            _aiLerp.autoRepath = new AutoRepathPolicy()
            {
                mode = AutoRepathPolicy.Mode.Dynamic,
                sensitivity = 1.5f,
            };
        }
    }

    private void OnDisable()
    {
        _speed = 0f;
        if (usePathfinding)
        {
            _aiLerp.SetPath(null);
            _aiLerp.destination = Vector3.positiveInfinity;
        }
    }

    private void FixedUpdate()
    {
        Chase();
        ApplyMovement();
    }

    private void Chase()
    {
        if (_target == null) return;

        var distanceToTarget = Vector2.Distance(_target.position, transform.position);

        if (distanceToTarget <= stoppingDistance) _speed = 0f;
        else if (distanceToTarget <= slowdownDistance) _speed = Mathf.Lerp(_speed, 0f, decelerationRate);
        else _speed = Mathf.MoveTowards(_speed, maxSpeed, acceleration * Time.fixedDeltaTime);

        var hasDirectPath = HasDirectPathToTarget;

        Vector2 directPathVelocity = Vector2.zero;
        Vector2 pathfinderVelocity = Vector2.zero;
        if (!usePathfinding || hasDirectPath)
        {
            if (distanceToTarget <= stoppingDistance) directPathVelocity = Vector2.zero;
            else if (distanceToTarget <= slowdownDistance) directPathVelocity = Vector2.Lerp(MoveVector, Vector2.zero, decelerationRate);
            else
            {
                var oldPos = (Vector2)transform.position;
                var newPos = Vector2.MoveTowards(transform.position, _target.position, _speed * Time.fixedDeltaTime);
                directPathVelocity = newPos - oldPos;
            }
        }
        if (usePathfinding)
        {
            _aiLerp.destination = _target.position;
            _aiLerp.speed = _speed;
            _aiLerp.MovementUpdate(Time.fixedDeltaTime, out var nextPosition, out _);
            pathfinderVelocity = nextPosition - transform.position;
        }

        if (!usePathfinding)
            MoveVector = directPathVelocity;
        else if (!hasDirectPath)
            MoveVector = pathfinderVelocity;
        else
            MoveVector = smoothingFactor * directPathVelocity + (1 - smoothingFactor) * pathfinderVelocity;
    }

    private void ApplyMovement()
    {
        if (!CalculateOnly)
        {
            _rb.position += MoveVector;
            transform.position += (Vector3)MoveVector;
        }
    }

    private bool HasDirectPathToTarget => !Physics2D.Linecast(transform.position, _target.position, terrainLayers);

    public void SetTarget(Transform target)
    {
        _target = target;
        if (usePathfinding) _aiLerp.destination = _target.position;
    }

    public void UnsetTarget()
    {
        _target = null;
        if (usePathfinding)
        {
            _aiLerp.SetPath(null);
            _aiLerp.destination = Vector3.positiveInfinity;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _target.position);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)MoveVector * 10f);
    }
#endif
}
