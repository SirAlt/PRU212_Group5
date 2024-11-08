public abstract class PlayerDeathState : PlayerState
{
    protected PlayerDeathState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        player.Hurtbox.Collider.enabled = false;
    }

    public override void ExitState()
    {
        base.ExitState();
        player.Hurtbox.Collider.enabled = true;
    }
}
