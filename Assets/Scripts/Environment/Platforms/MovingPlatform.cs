using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
[RequireComponent(typeof(Collider2D))]
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private LayerMask ferriableLayers;
    [SerializeField] private bool topsideOnly;

    private readonly List<IDraggable> _ferriedObjects = new();
    private Collider2D _detector;
    private IMover _mover;

    private void Awake()
    {
        _detector = GetComponent<Collider2D>();
        _mover = transform.parent.GetComponentInChildren<IMover>();
    }

    private void Start()
    {
        _detector.isTrigger = true;
    }

    private void FixedUpdate()
    {
        foreach (var obj in _ferriedObjects)
        {
            obj.MoveAlong(_mover.MoveVector);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((ferriableLayers & (1 << collision.gameObject.layer)) != 0)
        {
            if (topsideOnly && collision.bounds.min.y < _detector.bounds.min.y)
                return;

            var ferriableObj = collision.transform.parent.GetComponentInChildren<IDraggable>();
            if (ferriableObj != null)
            {
                _ferriedObjects.Add(ferriableObj);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!topsideOnly) return;

        if (collision.bounds.min.y < _detector.bounds.min.y)
        {
            var ferriableObj = collision.transform.parent.GetComponentInChildren<IDraggable>();
            if (ferriableObj != null)
            {
                _ferriedObjects.Remove(ferriableObj);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((ferriableLayers & (1 << collision.gameObject.layer)) != 0)
        {
            var ferriableObj = collision.transform.parent.GetComponentInChildren<IDraggable>();
            if (ferriableObj != null)
            {
                _ferriedObjects.Remove(ferriableObj);
            }
        }
    }
}
