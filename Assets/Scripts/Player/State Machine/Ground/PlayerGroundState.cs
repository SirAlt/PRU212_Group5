using UnityEngine;

public abstract class PlayerGroundState : PlayerState, IGroundState
{
    public PlayerGroundState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        if (stateMachine.PrevState is IGroundState) return;
        player.AirJumpCharges = player.Abilities.AirJumpCharges;
        player.AirDashCharges = player.Abilities.AirDashCharges;
    }

    public override void ExitState()
    {
        base.ExitState();
        if (stateMachine.NextState is IGroundState) return;
        player.FrameVelocity.y = 0; // Cancel grounding force.
        player.TimeLeftGround = Time.time;
    }

    public override void CheckForTransition()
    {
        base.CheckForTransition();
        if (stateMachine.Transitioning) return;
        if (!player.BodyContacts.Ground)
        {
            stateMachine.ChangeState(player.NaturalFallState);
            return;
        }
        if (player.HasValidDashInput)
        {
            stateMachine.ChangeState(player.GroundDashState);
            return;
        }
        if (player.BodyContacts.WhollyOnOneWayPlatform && player.Input.Move.y < 0 && player.HasJumpInput)
        {
            stateMachine.ChangeState(player.DropThroughFallState);
            return;
        }
        if (player.HasValidJumpInput)
        {
            stateMachine.ChangeState(player.GroundJumpState);
            return;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.FrameVelocity.y = -1.0f * player.Stats.GroundingVelocity;
    }
}
