using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SequenceCondition : Condition, ITriggerable
{
    [SerializeField] private List<GameObject> inputs = new();
    [SerializeField] private List<int> answer = new();

    private readonly List<ITriggerable> pins = new();
    private int _curStep;

    bool ITriggerable.IsOn => _curStep < answer.Count;

    void ITriggerable.TriggerOn() => CheckPuzzle();
    void ITriggerable.TriggerOff() => CheckPuzzle();

    private void OnEnable()
    {
        CheckPuzzle();
    }

    private void Start()
    {
        pins.AddRange(inputs.Select(e => e.GetComponentInChildren<ITriggerable>()));
    }

    private void CheckPuzzle()
    {
        if (_curStep < 0 || _curStep >= answer.Count) return;

        var activePinCount = 0;
        for (var i = 0; i < pins.Count; i++)
        {
            if (pins[i].IsOn) ++activePinCount;
        }

        if (activePinCount != 1) return;

        if (!pins[answer[_curStep]].IsOn)
        {
            _curStep = 0;
            // In case the current active (incorrect) pin is also the 1st one in the correct sequence.
            //if (pins[answer[0]].IsOn) ++_curStep;
        }
        else if (++_curStep == answer.Count)
        {
            NotifyConditionFulfilled();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (inputs != null && inputs.Count == 0)
        {
            Debug.LogWarning($"No control target set for [ {gameObject.name} ].", gameObject);
        }
        else if (inputs != null)
        {
            for (var i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].GetComponentInChildren<ITriggerable>() == null)
                    Debug.LogWarning($"Control target at index {i} of [ {gameObject.name} ] does not have a(n) {nameof(ITriggerable)} component.", gameObject);
            }
        }

        if (answer != null && answer.Count == 0)
        {
            Debug.LogWarning($"No or invalid answer has been set for {nameof(SequenceCondition)} on {gameObject.name}.", gameObject);
        }
        else if (answer != null)
        {
            for (var i = 0; i < answer.Count; i++)
            {
                if (answer[i] >= inputs.Count)
                {
                    Debug.LogWarning($"Invalid digit at index {i} of {nameof(answer)} for {nameof(SequenceCondition)} on {gameObject.name}.", gameObject);
                }
            }
        }
    }
#endif
}
