using System.Collections.Generic;
using UnityEngine;

public class DetectionZone : MonoBehaviour
{
    [SerializeField] private float trackingZoneMultiplier;

    [HideInInspector] public List<Collider2D> detected = new();
    private Collider2D _col;
    private Vector3 _originalScale;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true;
        _originalScale = transform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (detected.Contains(collision)) return;
        if (detected.Count == 0)
        {
            transform.localScale *= trackingZoneMultiplier;
        }
        detected.Add(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        detected.Remove(collision);
        if (detected.Count == 0)
        {
            _col.transform.localScale = Vector3.one;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        foreach (var collider in detected)
        {
            Gizmos.DrawLine(transform.position, collider.transform.position);
        }
    }
#endif
}