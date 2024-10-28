using System;
using UnityEngine;

public abstract class Condition : MonoBehaviour
{
    public event Action ConditionFulfilled;

    protected virtual void NotifyConditionFulfilled()
    {
        ConditionFulfilled?.Invoke();
    }

    public virtual void Activate()
    {
    }
}
