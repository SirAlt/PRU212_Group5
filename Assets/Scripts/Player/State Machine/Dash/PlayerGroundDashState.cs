public class PlayerGroundDashState : PlayerDashState, IGroundState
{
    public PlayerGroundDashState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        player.Animator.Play(PlayerController.GroundDashStartAnim, -1, 0f);
    }
}
