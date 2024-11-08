public class PlayerAirJumpState : PlayerJumpState
{
    public PlayerAirJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        player.Animator.Play(PlayerController.AirJumpAnim, -1, 0f);
    }

    public override void ExecuteJump()
    {
        --player.AirJumpCharges;
        player.FrameVelocity.y = player.Abilities.AirJumpPower > 0 ? player.Abilities.AirJumpPower : player.Stats.JumpPower;
    }
}
