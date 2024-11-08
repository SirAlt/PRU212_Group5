using System;
using System.Collections.Generic;
using UnityEngine;

public class ChainActivator : MonoBehaviour, ITriggerable
{
    [SerializeField] private List<GameObject> controlTargets = new();
    [SerializeField] private ChainedEvent eventsToChain;

    [Flags]
    private enum ChainedEvent
    {
        None = 0,
        Activate = 1,
        Deactivate = 2,
    }

    private readonly List<ITriggerable> _targets = new();

    bool ITriggerable.IsOn => enabled;

    void ITriggerable.TriggerOn() => ActivateAll();
    void ITriggerable.TriggerOff() => DeactivateAll();

    private void Awake()
    {
        foreach (var controlTarget in controlTargets)
        {
            _targets.Add(controlTarget.GetComponentInChildren<ITriggerable>());
        }
    }

    private void OnEnable()
    {
        if (eventsToChain.HasFlag(ChainedEvent.Activate))
            ActivateAll();
    }

    private void OnDisable()
    {
        if (eventsToChain.HasFlag(ChainedEvent.Deactivate))
            DeactivateAll();
    }

    private void ActivateAll()
    {
        _targets.ForEach(e => e.TriggerOn());
    }

    private void DeactivateAll()
    {
        _targets.ForEach(e => e.TriggerOff());
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
#endif
}
