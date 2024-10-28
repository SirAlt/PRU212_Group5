using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ProximityTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask triggerLayers;
    [SerializeField] private List<GameObject> controlTargets = new();
    [SerializeField] private float activationDelay;
    [SerializeField] private bool oneTime;

    private readonly List<ITriggerable> _payloads = new();
    private Collider2D _detector;

    private bool _isTriggering;

    private void Awake()
    {
        _detector = GetComponent<Collider2D>();

        foreach (var controlTarget in controlTargets)
        {
            _payloads.Add(controlTarget.GetComponentInChildren<ITriggerable>());
        }
    }

    private void OnEnable()
    {
        _isTriggering = false;
        _detector.enabled = true;
    }

    private void Start()
    {
        _detector.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isTriggering) return;
        if ((triggerLayers & (1 << collision.gameObject.layer)) != 0)
        {
            _isTriggering = true;
            if (oneTime) _detector.enabled = false;
            Invoke(nameof(DeliverPayload), activationDelay);
        }
    }

    private void DeliverPayload()
    {
        _isTriggering = false;
        foreach (var payload in _payloads) payload.TriggerOn();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (controlTargets != null && controlTargets.Count == 0)
        {
            Debug.LogWarning($"No control target set for [ {gameObject.name} ].", gameObject);
        }
        else if (controlTargets != null)
        {
            for (var i = 0; i < controlTargets.Count; i++)
            {
                if (controlTargets[i].GetComponentInChildren<ITriggerable>() == null)
                    Debug.LogWarning($"Control target at index {i} of [ {gameObject.name} ] does not have a(n) {nameof(ITriggerable)} component.", gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var controlTarget in controlTargets)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, controlTarget.transform.position);
        }
    }
#endif
}
