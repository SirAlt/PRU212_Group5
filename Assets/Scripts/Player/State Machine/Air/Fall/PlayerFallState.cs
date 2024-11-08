public abstract class PlayerFallState : PlayerAirState, IFallState
{
    public PlayerFallState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void CheckForTransition()
    {
        // PRIO: _Run_ > _Idle_ > ... > _WallSlide_ > AirJump
        if (player.BodyContacts.Ground)
        {
            player.Land();
            return;
        }

        base.CheckForTransition();

        if (stateMachine.Transitioning && stateMachine.NextState is not (PlayerAirJumpState or PlayerAirSlashState)) return;
        if (player.Abilities.WallSlideLearnt && player.IsMovingAgainstWall)
        {
            stateMachine.ChangeState(player.WallSlideState);
            return;
        }
    }
}
