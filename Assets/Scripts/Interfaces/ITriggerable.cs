public interface ITriggerable
{
    bool IsOn { get; }

    void TriggerOn();
    void TriggerOff();
}
