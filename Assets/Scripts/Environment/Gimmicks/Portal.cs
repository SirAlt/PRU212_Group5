using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private readonly HashSet<GameObject> _justPortaledObjects = new();

    [SerializeField] private LayerMask teleportableLayers;
    [SerializeField] private Transform destination;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((teleportableLayers & (1 << collision.gameObject.layer)) == 0) return;

        if (_justPortaledObjects.Contains(collision.gameObject)) return;

        if (destination.TryGetComponent<Portal>(out var destPortal))
        {
            destPortal._justPortaledObjects.Add(collision.gameObject);
        }
        collision.transform.position = destination.position;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((teleportableLayers & (1 << collision.gameObject.layer)) != 0)
        {
            _justPortaledObjects.Remove(collision.gameObject);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!destination) return;
        if (destination.TryGetComponent<Portal>(out _))
            Gizmos.color = Color.cyan;
        else
            Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, destination.position);
    }
#endif
}
