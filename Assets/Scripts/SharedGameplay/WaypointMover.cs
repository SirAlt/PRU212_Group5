using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-5)]
[RequireComponent(typeof(Rigidbody2D))]
public class WaypointMover : MonoBehaviour, IMover, ITriggerable
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float waypointReachedDistance = 0.1f;

    [SerializeField] private int startingWaypointIndex;
    [SerializeField] private bool teleportToStartingWaypoint;
    [SerializeField] private List<Transform> waypoints = new();

    [SerializeField] private List<float> restTimes = new();
    [SerializeField] private PathEndBehavior pathEndBehavior;

    private int _currentWaypointIndex;
    private Transform _currentWaypoint;
    private Rigidbody2D _rb;

    private bool _isResting;
    private bool _isStopped;
    private int _direction = 1;

    public Vector2 MoveVector { get; private set; }
    public bool CalculateOnly { get; set; }

    bool ITriggerable.IsOn => !_isStopped;

    void ITriggerable.TriggerOn() => StartMovement();
    void ITriggerable.TriggerOff() => StopMovement();

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        ValidateSettings();
    }

    private void OnEnable()
    {
        InitializeMovement();
    }

    private void InitializeMovement()
    {
        _currentWaypointIndex = startingWaypointIndex;
        _currentWaypoint = waypoints[_currentWaypointIndex];
        if (teleportToStartingWaypoint) transform.position = _currentWaypoint.position;
    }

    private void ValidateSettings()
    {
        foreach (var waypoint in waypoints)
        {
            if (waypoint.IsChildOf(transform))
            {
                Debug.LogWarning($"One of the {nameof(WaypointMover)} waypoints set for [ {gameObject.name} ] is itself or a child object.", gameObject);
                break;
            }
        }

        if (restTimes.Count != waypoints.Count)
        {
            Debug.LogWarning($"Different numbers of waypoints and rest times set for {nameof(WaypointMover)} of [ {gameObject.name} ].", gameObject);
            if (restTimes.Count < waypoints.Count)
            {
                restTimes.AddRange(new float[waypoints.Count - restTimes.Count]);
            }
        }

        // Not a typo. Only validate the rest times that have matching waypoints.
        for (var i = 0; i < waypoints.Count; i++)
        {
            if (restTimes[i] < 0f)
            {
                Debug.Log($"Negative rest time at index {i} set for {nameof(WaypointMover)} of [ {gameObject.name} ].", gameObject);
            }
        }

        if (startingWaypointIndex < 0)
        {
            Debug.LogWarning($"Negative starting waypoint set for {nameof(WaypointMover)} of [ {gameObject.name} ].", gameObject);
            startingWaypointIndex = 0;
        }
        else if (startingWaypointIndex >= waypoints.Count)
        {
            Debug.LogWarning($"Starting waypoint exceeds number of waypoints for {nameof(WaypointMover)} of [ {gameObject.name} ].", gameObject);
            startingWaypointIndex = waypoints.Count - 1;
        }

        // Do this check last since disabling movement changes the waypoint list, which could cause other checks to fail.
        switch (pathEndBehavior)
        {
            case PathEndBehavior.Reverse:
                if (waypoints.Count < 2)
                    DisableMovement();
                break;
            default:
                if (waypoints.Count < 1)
                    DisableMovement();
                break;
        }

        void DisableMovement()
        {
            moveSpeed = 0f;
            waypointReachedDistance = -1.0f;
            waypoints = new() { transform };
            startingWaypointIndex = 0;
            Debug.LogWarning($"{nameof(WaypointMover)} of [ {gameObject.name} ] does not meet the required minimum number of waypoints for the selected path end behavior.", gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (_isResting || _isStopped) return;

        // HACK:
        // 1. Setting Rigidbody position fails for nested moving platforms (e.g., moving platforms inside moving rooms).
        //    This is because a Rigidbody controls the world-space position, and never the local-space position, of its GameObject's Transform.
        //    This means the Rigidbody of the parent moving platform overrides that of its children's.
        // 2. Setting the Transform's position (either world- or local-space) causes our physics scripts to fail.
        //    Our scripts rely on Rigidbodies and Colliders, but setting a GameObject's Transform does not update its components' positions until
        //    Unity's internal physics update phase, which is after all user scripts' FixedUpdate calls.
        // 
        // => We set both the positions of the Rigidbody and the Transform. The call to Rigidbody must be made first, so that it'll be overriden by
        //    the call to its own Transform and not the one to parent's Rigidbody.

        var oldPos = (Vector2)transform.position;
        var newPos = Vector2.MoveTowards(transform.position, _currentWaypoint.position, moveSpeed * Time.fixedDeltaTime);
        MoveVector = newPos - oldPos;

        if (!CalculateOnly)
        {
            _rb.position += MoveVector;          // This is to move colliders immediately.
            transform.position += (Vector3)MoveVector;    // And this is to allow nested moving platforms.
        }

        // Update movement vector for interface.

        if (Vector2.Distance(transform.position, _currentWaypoint.position) <= waypointReachedDistance)
        {
            _isResting = true;
            MoveVector = Vector2.zero;
            if (restTimes[_currentWaypointIndex] >= 0f)
                // Use Invoke() instead of coroutine so this goes through even if we get disabled while resting.
                Invoke(nameof(GetNextWaypoint), restTimes[_currentWaypointIndex]);
            else
                _isStopped = true;
        }
    }

    private void GetNextWaypoint()
    {
        switch (pathEndBehavior)
        {
            case PathEndBehavior.Loop:
                _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Count;
                break;
            case PathEndBehavior.Reverse:
                if (_currentWaypointIndex <= 0)
                    _direction = 1;
                else if (_currentWaypointIndex >= waypoints.Count - 1)
                    _direction = -1;
                _currentWaypointIndex += _direction;
                break;
            case PathEndBehavior.Stop:
                if (_currentWaypointIndex < waypoints.Count - 1)
                    ++_currentWaypointIndex;
                break;
        }

        _currentWaypoint = waypoints[_currentWaypointIndex];
        _isResting = false;
    }

    private void StartMovement()
    {
        if (!_isStopped) return;
        _isStopped = false;

        if (Vector2.Distance(transform.position, _currentWaypoint.position) <= waypointReachedDistance)
            GetNextWaypoint();
    }

    private void StopMovement()
    {
        _isStopped = true;
        MoveVector = Vector2.zero;
    }

    private enum PathEndBehavior
    {
        Loop,
        Reverse,
        Stop,
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        switch (pathEndBehavior)
        {
            case PathEndBehavior.Loop:
                Gizmos.color = Color.blue;
                break;
            case PathEndBehavior.Reverse:
                Gizmos.color = Color.magenta;
                break;
            case PathEndBehavior.Stop:
                Gizmos.color = Color.black;
                break;
        }

        var count = waypoints.Count;
        if (pathEndBehavior != PathEndBehavior.Loop)
            --count;
        for (int i = 0; i < count; i++)
        {
            var from = i;
            var to = (i + 1) < waypoints.Count ? (i + 1) : 0;
            Gizmos.DrawLine(waypoints[from].position, waypoints[to].position);
        }

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, waypoints[startingWaypointIndex].position);
    }
#endif
}
