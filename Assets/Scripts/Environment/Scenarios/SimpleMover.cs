using Pathfinding;
using System;
using UnityEngine;

public class SimpleMover : MonoBehaviour, IMover
{
    [SerializeField] private float acceleration;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float slowdownDistance;

    private Rigidbody2D _rb;
    [SerializeField] private Transform _target;
    private AILerp _aiLerp;
    private float _speed;

    public Vector2 MoveVector { get; private set; }
    public bool CalculateOnly { get; set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _aiLerp = GetComponent<AILerp>();
    }

    private void Start()
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

    private void OnDisable()
    {
        _speed = 0f;
        _aiLerp.SetPath(null);
        _aiLerp.destination = Vector3.positiveInfinity;
    }

    private void FixedUpdate()
    {
        Follow();
        ApplyMovement();
    }

    private void Follow()
    {
        var distanceToTarget = Vector2.Distance(_target.position, transform.position);
        if (distanceToTarget <= slowdownDistance) _speed = Mathf.Lerp(_speed, 0f, 0.05f);
        else _speed = Mathf.MoveTowards(_speed, maxSpeed, acceleration * Time.fixedDeltaTime);

        _aiLerp.destination = _target.position;
        _aiLerp.speed = _speed;
        _aiLerp.MovementUpdate(Time.fixedDeltaTime, out var nextPosition, out _);
        MoveVector = nextPosition - transform.position;
    }

    private void ApplyMovement()
    {
        if (!CalculateOnly)
        {
            _rb.position += MoveVector;
            transform.position += (Vector3)MoveVector;
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        _aiLerp.destination = _target.position;
    }

    public void UnsetTarget()
    {
        _target = null;
        _aiLerp.SetPath(null);
        _aiLerp.destination = Vector3.positiveInfinity;
    }
}
