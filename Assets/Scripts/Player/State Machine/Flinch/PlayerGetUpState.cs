using UnityEngine;

public class PlayerGetUpState : PlayerState
{
    public PlayerGetUpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        player.Input.ClearJump();
        player.FrameVelocity.y = player.Stats.GetUpFrameCount * player.Stats.GravitationalAcceleration * Time.fixedDeltaTime;
        if (player.Stats.InvincibleGetUp) player.Hurtbox.GainInvincibility(player.Stats.GetUpFrameCount * Time.fixedDeltaTime);
        player.Animator.Play(PlayerController.RecoveryJumpAnim, -1, 0f);
    }

    public override void CheckForTransition()
    {
        base.CheckForTransition();
        if (stateMachine.Transitioning) return;
        if (player.FrameVelocity.y <= 0)
        {
            stateMachine.ChangeState(player.JumpFallState);
            return;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.FrameVelocity.y = Mathf.MoveTowards(player.FrameVelocity.y, -1.0f * player.Stats.FallSpeedClamp, player.Stats.GravitationalAcceleration * Time.fixedDeltaTime);
    }

    protected override void UpdateFacing()
    {
        // Do nothing. Turning is not allowed.
    }
}
