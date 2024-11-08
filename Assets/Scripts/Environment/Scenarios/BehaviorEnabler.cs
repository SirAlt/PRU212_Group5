using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BehaviorEnabler : MonoBehaviour, ITriggerable
{
    [SerializeField] private List<Behaviour> _controlTargets;
    [SerializeField] private bool _startEnabled;

    private void Start()
    {
        _controlTargets.ForEach(e => e.enabled = _startEnabled);
    }

    bool ITriggerable.IsOn => _controlTargets.All(e => e.enabled);

    void ITriggerable.TriggerOn() => _controlTargets.ForEach(e => e.enabled = true);
    void ITriggerable.TriggerOff() => _controlTargets.ForEach(e => e.enabled = false);
}
