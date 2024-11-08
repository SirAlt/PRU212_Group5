using UnityEngine;

public class PlayerWallSlideState : PlayerWallState
{
    public PlayerWallSlideState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        player.FrameVelocity.y = 0;
        player.Animator.Play(PlayerController.WallSlideAnim, -1, 0f);
    }

    public override void CheckForTransition()
    {
        base.CheckForTransition();
        if (!player.IsMovingAgainstWall)
        {
            stateMachine.ChangeState(player.NaturalFallState);
            return;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        var _accel = player.Abilities.WallSlideGravityModifier * player.Stats.GravitationalAcceleration;
        var _speedClamp = player.Abilities.WallSlideGravityModifier * player.Stats.FallSpeedClamp;
        player.FrameVelocity.y = Mathf.MoveTowards(player.FrameVelocity.y, -1.0f * _speedClamp, _accel * Time.fixedDeltaTime);
    }
}
