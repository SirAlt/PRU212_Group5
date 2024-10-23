using Assets.Scripts;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlyingEye : Enemy
{
    [SerializeField] private float flightSpeed = 2f;
    [SerializeField] private float waypointReachedDistance = 0.2f;
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private DetectionZone biteDetectionZone;
    [SerializeField] private EnemyFacing facing;
    [SerializeField] private Transform darkPoint;
    [SerializeField] private GameObject[] darkballs;

    private Animator animator;
    private Rigidbody2D rb;

    public bool _hasTarget = false;

    Transform nextWaypoint;
    int waypointNum = 0;

    public BodyContacts BodyContacts { get; private set; }

    public bool HasTarget
    {
        get { return _hasTarget; }
        private set
        {
            _hasTarget = value;
            animator.SetBool(AnimationStrings.hasTarget, value);
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        facing = GetComponent<EnemyFacing>();
        BodyContacts = GetComponent<BodyContacts>();
    }

    private void Start()
    {
        nextWaypoint = waypoints[waypointNum];
    }

    void Update()
    {
        if (biteDetectionZone != null)
        {
            HasTarget = biteDetectionZone.detectedColliders.Count > 0;
            //Debug.Log("Detected target: " + HasTarget);

            if (HasTarget)
            {
                // V? tr� vi�n ??n
                darkballs[FindDarkball()].transform.position = darkPoint.position;
                darkballs[FindDarkball()].GetComponent<DarkBall>().SetDirection(Mathf.Sign(transform.localScale.x));
                // D?ng chuy?n ??ng khi c� m?c ti�u
                rb.velocity = Vector2.zero;

                // Quay m?t v? h??ng m?c ti�u
                if (biteDetectionZone.detectedColliders.Count > 0)
                {
                    Collider2D targetCollider = biteDetectionZone.detectedColliders[0]; // L?y collider ??u ti�n
                    Vector2 directionToTarget = (targetCollider.transform.position - transform.position).normalized;
                    facing.FaceRight(); // N?u h??ng di chuy?n l� b�n ph?i
                    if (directionToTarget.x < 0)
                    {
                        facing.FaceLeft(); // N?u h??ng di chuy?n l� b�n tr�i
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("biteDetectionZone is not assigned in the Inspector.");
        }
    }

    private int FindDarkball()
    {
        for (int i = 0; i < darkballs.Length; i++)
        {
            if (!darkballs[i].activeInHierarchy)
            {
                return i;
            }
        }
        return 0;
    }

    private void FixedUpdate()
    {
        if (!HasTarget)
        {
            Flight();
        }
    }

    private void Flight()
    {
        // T�nh to�n kho?ng c�ch ??n waypoint hi?n t?i
        float distance = Vector2.Distance(nextWaypoint.position, transform.position);

        // X�c ??nh h??ng di chuy?n t?i waypoint
        Vector2 directionToWaypoint = (nextWaypoint.position - transform.position).normalized;

        // X? l� va ch?m v?i t??ng ho?c m?t ??t: chuy?n sang waypoint ti?p theo n?u ch?m
        if (BodyContacts.WallLeft || BodyContacts.WallRight || BodyContacts.Ground)
        {
            // N?u ch?m t??ng ho?c m?t ??t, quay l?i waypoint ??u ti�n
            waypointNum = 0; // Quay l?i waypoint ??u ti�n
            nextWaypoint = waypoints[waypointNum];
            directionToWaypoint = (nextWaypoint.position - transform.position).normalized;
        }

        // X? l� va ch?m v?i tr?n: ?i?u ch?nh h??ng bay xu?ng
        if (BodyContacts.Ceiling)
        {
            directionToWaypoint.y = -Mathf.Abs(directionToWaypoint.y); // Bay xu?ng t? tr�n
        }

        // X�c ??nh h??ng di chuy?n
        if (directionToWaypoint.x < 0)
            facing.FaceLeft();
        else if (directionToWaypoint.x > 0)
            facing.FaceRight();

        // C?p nh?t v?n t?c c?a Rigidbody d?a tr�n h??ng di chuy?n
        rb.velocity = directionToWaypoint * flightSpeed;

        // Ki?m tra n?u ?� ??n g?n waypoint v� chuy?n sang waypoint k? ti?p
        if (distance <= waypointReachedDistance)
        {
            waypointNum++;
            if (waypointNum >= waypoints.Count)
            {
                waypointNum = 0;
            }
            nextWaypoint = waypoints[waypointNum];
        }
    }
}
