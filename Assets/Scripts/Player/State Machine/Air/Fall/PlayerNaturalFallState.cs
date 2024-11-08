using UnityEngine;

public class PlayerNaturalFallState : PlayerFallState
{
    public PlayerNaturalFallState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        if (stateMachine.PrevState is not PlayerFallState)
            player.Animator.Play(PlayerController.NaturalFallAnim, -1, 0f);
    }

    public override void CheckForTransition()
    {
        // PRIO: ... > _(Coyote)WallJump_ > _(Coyote)GroundJump_ > AirJump
        base.CheckForTransition();
        if (stateMachine.Transitioning && stateMachine.NextState is not (PlayerAirJumpState or PlayerAirSlashState)) return;
        if (player.HasValidJumpInput)
        {
            // If we were touching a wall, [ Air ] would've already initiated a wall jump. No need to recheck body contacts.
            if (player.Abilities.WallJumpLearnt && player.TimeLeftWall + player.Stats.WallCoyoteTime > Time.time)
            {
                stateMachine.ChangeState(player.WallJumpState);
                return;
            }
            if (player.TimeLeftGround + player.Stats.CoyoteTime > Time.time)
            {
                stateMachine.ChangeState(player.GroundJumpState);
                return;
            }
        }
    }
}
