using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomSelectionActivator : MonoBehaviour, ITriggerable
{
    [SerializeField] private List<GameObject> controlTargets = new();
    [SerializeField] private int activationCount;

    private readonly List<ITriggerable> _targets = new();

    bool ITriggerable.IsOn => _targets.Count > 0 && activationCount > 0;

    void ITriggerable.TriggerOn()
    {
        ActivateRandomly();
    }

    void ITriggerable.TriggerOff()
    {
    }

    private void Awake()
    {
        foreach (var controlTarget in controlTargets)
        {
            _targets.Add(controlTarget.GetComponentInChildren<ITriggerable>());
        }
    }

    private void Start()
    {
        activationCount = Math.Clamp(activationCount, 0, _targets.Count);
        foreach (var target in _targets)
        {
            target.TriggerOff();
        }
    }

    private void ActivateRandomly()
    {
        // Random selection algo
        // cf. https://stackoverflow.com/questions/35065764/select-n-records-at-random-from-a-set-of-n
        var numer = activationCount;
        var denom = _targets.Count;
        int idx;
        for (idx = 0; idx < _targets.Count; idx++)
        {
            var roll = UnityEngine.Random.Range(1, denom + 1);
            if (roll <= numer)
            {
                _targets[idx].TriggerOn();
                if (--numer == 0) break;
            }
            else
            {
                _targets[idx].TriggerOff();
            }
            --denom;
        }
        for (++idx; idx < _targets.Count; idx++)
        {
            _targets[idx].TriggerOff();
        }
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

            if (activationCount == 0)
                Debug.LogWarning($"Activation count of {nameof(RandomSelectionActivator)} on {gameObject.name} is zero.", gameObject);
            else if (activationCount < 0 || activationCount > controlTargets.Count)
                Debug.LogWarning($"Invalid activation count of {nameof(RandomSelectionActivator)} on {gameObject.name}.", gameObject);
        }
    }
#endif
}
