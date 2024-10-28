using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(Collider2D))]
public class BodyContacts : MonoBehaviour
{
    [field: SerializeField] public CollisionConfigs Configurations { get; private set; }

    private Collider2D _collider;

    private ContactFilter2D _groundFilter;
    private ContactFilter2D _ceilingFilter;
    private ContactFilter2D _wallFilter;
    private ContactFilter2D _oneWayPlatformFilter;

    private readonly List<RaycastHit2D> _hits = new();
    private readonly List<Collider2D> _overlaps = new();

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();

        _groundFilter = new ContactFilter2D();
        _ceilingFilter = new ContactFilter2D();
        _wallFilter = new ContactFilter2D();
        _oneWayPlatformFilter = new ContactFilter2D();

        _groundFilter.SetLayerMask(Configurations.GroundLayer);
        _ceilingFilter.SetLayerMask(Configurations.CeilingLayer);
        _wallFilter.SetLayerMask(Configurations.WallLayer);
        _oneWayPlatformFilter.SetLayerMask(Configurations.OneWayPlatformLayer);
    }

    private void FixedUpdate()
    {
        var _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

        CheckSolidGround();
        CheckCeiling();
        CheckWall();
        CheckOneWayPlatform();

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    public bool SolidGround { get; private set; }
    public bool Ceiling { get; private set; }
    public bool WallLeft { get; private set; }
    public bool WallRight { get; private set; }
    public bool OnOneWayPlatform { get; private set; }
    public bool ThroughOneWayPlatform { get; private set; }

    public bool Ground => SolidGround || (!DropThrough && OnOneWayPlatform);
    public bool Wall => WallLeft || WallRight;
    public bool WhollyOnOneWayPlatform => OnOneWayPlatform && !SolidGround;

    public bool DropThrough { get; set; }

    private void CheckSolidGround()
    {
        Physics2D.queriesStartInColliders = true;
        SolidGround = _collider.Cast(Vector2.down, _groundFilter, _hits, Configurations.GroundTouchDistance) > 0;
        Debug.DrawLine(_collider.bounds.center, new Vector2(_collider.bounds.center.x, _collider.bounds.min.y - Configurations.GroundTouchDistance));
    }

    private void CheckCeiling()
    {
        Physics2D.queriesStartInColliders = true;
        Ceiling = _collider.Cast(Vector2.up, _ceilingFilter, _hits, Configurations.CeilingTouchDistance) > 0;
    }

    private void CheckWall()
    {
        Physics2D.queriesStartInColliders = true;
        WallRight = _collider.Cast(Vector2.right, _wallFilter, _hits, Configurations.WallTouchDistance) > 0;
        WallLeft = _collider.Cast(Vector2.left, _wallFilter, _hits, Configurations.WallTouchDistance) > 0;
    }

    private void CheckOneWayPlatform()
    {
        Physics2D.queriesStartInColliders = false;
        OnOneWayPlatform = _collider.Cast(Vector2.down, _oneWayPlatformFilter, _hits, Configurations.GroundTouchDistance) > 0;
        Physics2D.queriesStartInColliders = true;
        ThroughOneWayPlatform = _collider.OverlapCollider(_oneWayPlatformFilter, _overlaps) > 0;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Configurations == null) Debug.LogWarning($"Please assign a(n) {nameof(CollisionConfigs)} asset to the Body Contacts' Configurations slot of [ {gameObject.name} ].", gameObject);
    }
#endif
}
