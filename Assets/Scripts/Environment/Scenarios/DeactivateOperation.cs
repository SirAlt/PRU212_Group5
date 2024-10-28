using System.Collections.Generic;
using UnityEngine;

public class DeactivateOperation : Operation
{
    [SerializeField] private List<GameObject> controlTargets = new();

    private readonly List<ITriggerable> _targets = new();

    private void Awake()
    {

        foreach (var controlTarget in controlTargets)
        {
            _targets.Add(controlTarget.GetComponentInChildren<ITriggerable>());
        }
    }

    public override void Execute()
    {
        foreach (var target in _targets)
        {
            if (target != null) target.TriggerOff();
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
        }
    }
#endif
}
