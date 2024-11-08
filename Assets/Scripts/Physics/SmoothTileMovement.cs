using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class SmoothTileMovement : MonoBehaviour, IMovement
{
    [field: SerializeField] public CollisionConfigs Configurations { get; private set; }

    private ContactFilter2D _xFilter;
    private ContactFilter2D _yFilter;
    private readonly List<RaycastHit2D> _hits = new();

    private float minDot;   // Dot product of 2 normalized vectors where the angle between them is MAX_SLOPE_ANGLE.

    private Rigidbody2D _rb;

    private Collider2D _collider;
    public Collider2D Collider
    {
        set
        {
            _collider = value;
            _rb = _collider.attachedRigidbody;
        }
    }

    public Vector2 EnvironmentVelocity { get; set; }

    public void AddLayersX(LayerMask newLayers) => _xFilter.layerMask |= newLayers;
    public void AddLayersY(LayerMask newLayers) => _yFilter.layerMask |= newLayers;
    public void RemoveLayersX(LayerMask layersToRemove) => _xFilter.layerMask &= ~layersToRemove;
    public void RemoveLayersY(LayerMask layersToRemove) => _yFilter.layerMask &= ~layersToRemove;

    private void Awake()
    {
        _xFilter = new ContactFilter2D();
        _xFilter.SetLayerMask(Configurations.ObstacleLayer | Configurations.OneWayPlatformLayer);
        _xFilter.useTriggers = false;

        _yFilter = new ContactFilter2D();
        _yFilter.SetLayerMask(Configurations.ObstacleLayer | Configurations.OneWayPlatformLayer);
        _yFilter.useTriggers = false;

        minDot = Mathf.Cos(Configurations.MaxSlopeAngle * Mathf.Deg2Rad);
    }

    public void Move(Vector2 velocity)
    {
        var cachedQueriesStartInColliders = Physics2D.queriesStartInColliders;
        Physics2D.queriesStartInColliders = true;

        var totalVelocity = velocity + EnvironmentVelocity;

        // TODO: Use Collider shape instead
        var originalBounds = GetBounds();
        var bounds = originalBounds;
        StepX();

        bounds = GetBounds();
        StepY();

        Physics2D.queriesStartInColliders = cachedQueriesStartInColliders;

        void StepX()
        {
            var xStep = totalVelocity.x * Time.fixedDeltaTime;
            if (xStep == 0) return;

            bool terrainHit = _collider.Cast(Vector2.right * Mathf.Sign(xStep), _xFilter, _hits, Mathf.Abs(xStep) + Configurations.CollisionOffset) > 0;

            if (terrainHit)
            {
                EliminateOneWayPlatforms();
                if (_hits.Count == 0) goto APPLY_MOVEMENT;

                float distance;
                var closestDistance = Mathf.Infinity;
                RaycastHit2D closestHit = _hits[0];
                foreach (var hit in _hits)
                {
                    distance = xStep > 0
                       ? hit.point.x - bounds.max.x
                       : bounds.min.x - hit.point.x;
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestHit = hit;
                    }
                }

                var dot = Vector2.Dot(closestHit.normal, Vector2.up);
                if (dot >= minDot && dot < 1)
                {
                    //var oldPos = _rb.position;

                    var bodyXStep = velocity.x * Time.fixedDeltaTime;
                    var envXStep = EnvironmentVelocity.x * Time.fixedDeltaTime;

                    var step = -1.0f * bodyXStep * Vector2.Perpendicular(closestHit.normal) + new Vector2(envXStep, 0);
                    _rb.position += step;

                    //Debug.Log($"Slope movement:  oldPos={oldPos}  step={step}  newPos={_rb.position}  minDot={minDot}");
                    return;
                }
                else
                {
                    xStep = Mathf.Sign(xStep) * (closestDistance - Configurations.CollisionOffset);
                }
            }
        APPLY_MOVEMENT:
            _rb.position = new Vector2(_rb.position.x + xStep, _rb.position.y);
        }

        void StepY()
        {
            var yStep = totalVelocity.y * Time.fixedDeltaTime;
            if (yStep == 0) return;

            bool terrainHit = _collider.Cast(Vector2.up * Mathf.Sign(yStep), _yFilter, _hits, Mathf.Abs(yStep) + Configurations.CollisionOffset) > 0;

            if (terrainHit)
            {
                EliminateOneWayPlatforms();
                if (_hits.Count == 0) goto APPLY_MOVEMENT;

                float distance;
                var closestDistance = Mathf.Infinity;
                foreach (var hit in _hits)
                {
                    distance = yStep > 0
                       ? hit.point.y - bounds.max.y
                       : bounds.min.y - hit.point.y;
                    if (distance < closestDistance) { closestDistance = distance; }
                }
                yStep = Mathf.Sign(yStep) * (closestDistance - Configurations.CollisionOffset);
            }
        APPLY_MOVEMENT:
            _rb.position = new Vector2(_rb.position.x, _rb.position.y + yStep);
        }

        void EliminateOneWayPlatforms()
        {
            for (int i = _hits.Count - 1; i >= 0; --i)
            {
                var hit = _hits[i];
                if ((Configurations.OneWayPlatformLayer & (1 << hit.transform.gameObject.layer)) != 0)
                {
                    if (Mathf.Abs(hit.point.x - bounds.center.x) <= bounds.extents.x
                        && Mathf.Abs(hit.point.y - bounds.center.y) <= bounds.extents.y)
                    {
                        _hits.Remove(hit);
                        continue;
                    }

                    var dot = Vector2.Dot(hit.normal, Vector2.up);
                    if (dot <= 0 /*|| (dot == 1 && originalBounds.min.y <= hit.collider.bounds.max.y)*/)
                    {
                        _hits.Remove(hit);
                        continue;
                    }
                }
            }
        }
    }

    private Bounds GetBounds()
    {
        return _collider.bounds;

        //var colliders = new List<Collider2D>();
        //_ = _rb.GetAttachedColliders(colliders);

        //if (colliders.Count > 0)
        //{
        //    var bounds = colliders[0].bounds;
        //    for (int i = 1; i < colliders.Count; i++)
        //    {
        //        bounds.Encapsulate(colliders[i].bounds);
        //    }
        //    return bounds;
        //}
        //return new Bounds(_rb.position, Vector3.zero);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Configurations == null) Debug.LogWarning($"Please assign a(n) {nameof(CollisionConfigs)} asset to the Movement script's Configurations slot of [ {gameObject.name} ].", gameObject);
    }
#endif
}
