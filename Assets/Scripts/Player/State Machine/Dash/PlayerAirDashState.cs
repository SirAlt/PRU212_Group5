public class PlayerAirDashState : PlayerDashState, IAirState
{
    public PlayerAirDashState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();

        --player.AirDashCharges;

        player.Animator.Play(PlayerController.AirDashStartAnim, -1, 0f);
    }

    public override void CheckForTransition()
    {
        // PRIO: _WallJump_ > _WallSlide_ > ...

        if (player.BodyContacts.Wall)
        {
            if (player.Stats.CanJumpCancelDash && player.Abilities.WallJumpLearnt && player.HasValidJumpInput)
            {
                stateMachine.ChangeState(player.WallJumpState);
                return;
            }
            if (player.Abilities.WallSlideLearnt && player.IsMovingAgainstWall)
            {
                stateMachine.ChangeState(player.WallSlideState);
                return;
            }
        }

        base.CheckForTransition();
    }

    #region Air State

    public void HandleAirControl()
    {
        // Do nothing. Character cannot move while dashing.
    }

    public void HandleGravity()
    {
        // Do nothing. Gravity is ignored while dashing.
    }

    #endregion
}
