public interface IJumpState
{
    float ApexRatio { get; }

    void ExecuteJump();
    void ApplyApexModifier();
}
