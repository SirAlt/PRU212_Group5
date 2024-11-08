using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingWindowActivator : MonoBehaviour
{
    [SerializeField] private List<GameObject> controlTargets = new();
    [SerializeField] private int numberOfActiveSets;
    [SerializeField] private float interval;
    [SerializeField] private float transitionLength;

    private readonly List<ITriggerable> _targets = new();
    private int _activeIdx;
    private float _timer;

    private void Awake()
    {
        foreach (var controlTarget in controlTargets)
        {
            _targets.Add(controlTarget.GetComponentInChildren<ITriggerable>());
        }
    }

    private void Start()
    {
        if (_targets.Count == 0) return;

        numberOfActiveSets = Math.Clamp(numberOfActiveSets, 0, _targets.Count);
        for (int i = 0; i < numberOfActiveSets; i++)
        {
            _targets[i].TriggerOn();
        }
        for (int i = numberOfActiveSets; i < _targets.Count; i++)
        {
            _targets[i].TriggerOff();
        }
    }

    private void FixedUpdate()
    {
        if (_targets.Count == 0) return;

        _timer += Time.fixedDeltaTime;
        if (_timer >= interval)
        {
            _timer = 0;
            StartCoroutine(nameof(DeactivateSet), _activeIdx);
            _targets[Clamp(_activeIdx + numberOfActiveSets)].TriggerOn();
            _activeIdx = Clamp(++_activeIdx);
        }
    }

    private IEnumerator DeactivateSet(int idx)
    {
        yield return new WaitForSeconds(transitionLength);
        _targets[idx].TriggerOff();
    }

    private int Clamp(int idx) => idx % _targets.Count;

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

        if (transitionLength < 0 || transitionLength >= interval)
        {
            Debug.LogWarning($"Invalid transition length on disappearing platform coordinator of [ {gameObject.name} ]. Should be non-negative and smaller than activation interval.", gameObject);
        }
    }
#endif
}
