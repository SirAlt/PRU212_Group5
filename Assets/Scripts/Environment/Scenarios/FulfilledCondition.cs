public class FulfilledCondition : Condition
{
    public override void Activate()
    {
        NotifyConditionFulfilled();
    }
}
