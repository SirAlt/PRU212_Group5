using System.Collections;
using UnityEngine;

public class TimeCondition : Condition
{
    [SerializeField] private float duration;

    public override void Activate()
    {
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(duration);
        NotifyConditionFulfilled();
    }
}
