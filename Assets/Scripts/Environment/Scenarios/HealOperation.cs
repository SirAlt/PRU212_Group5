using System.Collections.Generic;
using UnityEngine;

public class HealOperation : Operation
{
    [SerializeField] private List<GameObject> targets;
    [SerializeField] private List<float> amounts;

    public override void Execute()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i].GetComponentInChildren<IHealable>();
            var healing = i < amounts.Count ? amounts[i] : target.MaxHealth;
            target.Heal(healing > 0f ? healing : target.MaxHealth);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (targets != null && targets.Count == 0)
        {
            Debug.LogWarning($"No target set for {nameof(HealOperation)} on  [ {gameObject.name} ].", gameObject);
        }
        else if (targets != null)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].GetComponentInChildren<IHealable>() == null)
                {
                    Debug.LogWarning($"Target at index {i} of {nameof(HealOperation)} on [ {gameObject.name} ] does not have an IHealable component.", gameObject);
                }
            }
            if (amounts != null && targets.Count != amounts.Count)
            {
                Debug.LogWarning($"Mismatching numbers of targets and healing amounts for {nameof(HealOperation)} on [ {gameObject.name} ].", gameObject);
            }
        }
    }
#endif
}
