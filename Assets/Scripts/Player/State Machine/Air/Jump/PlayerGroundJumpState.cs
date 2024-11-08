public class PlayerGroundJumpState : PlayerJumpState
{
    public PlayerGroundJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        player.Animator.Play(PlayerController.GroundJumpAnim, -1, 0f);
    }

    public override void ExecuteJump()
    {
        player.FrameVelocity.y = player.Stats.JumpPower;
    }
}
