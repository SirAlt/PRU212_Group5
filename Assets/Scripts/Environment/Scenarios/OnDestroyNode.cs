public class OnDestroyNode : Node
{
    private bool _onceActivated;

    // We wanted this to be a Condition, but that wouldn't work since Unity
    // unsets all of an object's events when it is marked for destruction.
    private void OnDestroy()
    {
        // Base class's OnDisable() unsets activation state (to allow a later re-activation).
        // We need to force this to true to resolve success.
        if (_onceActivated) _activated = true;
        Fulfill();
    }

    protected override void OnSetup()
    {
        base.OnSetup();
        _onceActivated = true;
    }

    protected new void OnEnable()
    {
        base.OnEnable();
        _onceActivated = false;
    }
}
