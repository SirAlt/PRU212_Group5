public class PlayerDeathCondition : Condition
{
    private void OnEnable()
    {
        CharacterEvents.characterDied += NotifyConditionFulfilled;
    }

    private void OnDisable()
    {
        CharacterEvents.characterDied -= NotifyConditionFulfilled;
    }
}
