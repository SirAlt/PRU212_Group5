using UnityEngine;

public abstract class PlayerAirState : PlayerState, IAirState
{
    protected float gravity;

    public PlayerAirState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void CheckForTransition()
    {
        base.CheckForTransition();
        if (stateMachine.Transitioning) return;
        if (player.HasValidDashInput && player.AirDashCharges > 0)
        {
            stateMachine.ChangeState(player.AirDashState);
            return;
        }
        if (player.HasValidJumpInput)
        {
            if (player.Abilities.WallJumpLearnt && player.BodyContacts.Wall)
            {
                stateMachine.ChangeState(player.WallJumpState);
                return;
            }
            else if (player.AirJumpCharges > 0)
            {
                stateMachine.ChangeState(player.AirJumpState);
                return;
            }
        }
        if (player.HasValidSlashInput)
        {
            stateMachine.ChangeState(player.AirSlashState);
            return;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        HandleAirControl();
        HandleGravity();
    }

    public virtual void HandleAirControl()
    {
        if (player.Input.Move.x == 0)
        {
            player.FrameVelocity.x = Mathf.MoveTowards(player.FrameVelocity.x, 0, player.Stats.AirDeceleration * Time.fixedDeltaTime);
        }
        else
        {
            player.FrameVelocity.x = Mathf.MoveTowards(player.FrameVelocity.x,
                                                       player.Stats.MaxAirSpeed * Mathf.Sign(player.Input.Move.x),
                                                       player.Stats.GroundAcceleration * Time.fixedDeltaTime);
        }
    }

    public virtual void HandleGravity()
    {
        gravity = player.Stats.GravitationalAcceleration;
        player.FrameVelocity.y = Mathf.MoveTowards(player.FrameVelocity.y, -1.0f * player.Stats.FallSpeedClamp, gravity * Time.fixedDeltaTime);
    }
}
