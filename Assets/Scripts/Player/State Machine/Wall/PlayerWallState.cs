using UnityEngine;

public abstract class PlayerWallState : PlayerState, IWallState
{
    public PlayerWallState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        if (player.Abilities.RechargeJumpsOnWall)
            player.AirJumpCharges = player.Abilities.AirJumpCharges;
        if (player.Abilities.RechargeDashesOnWall)
            player.AirDashCharges = player.Abilities.AirDashCharges;
    }

    public override void ExitState()
    {
        base.ExitState();
        if (stateMachine.NextState is IWallState) return;
        player.TimeLeftWall = Time.time;
        player.LastWallContactSide = player.BodyContacts.WallLeft ? -1 : 1; // Prioritize left-to-right movement -> Prioritize left wall contact.
    }

    public override void CheckForTransition()
    {
        base.CheckForTransition();
        if (stateMachine.Transitioning) return;
        if (player.BodyContacts.Ground)
        {
            player.Land();
            return;
        }
        if (player.HasValidDashInput && player.AirDashCharges > 0)
        {
            stateMachine.ChangeState(player.AirDashState);
            return;
        }
        if (!player.BodyContacts.Wall)
        {
            stateMachine.ChangeState(player.NaturalFallState);
            return;
        }
        if (player.Abilities.WallJumpLearnt && player.HasValidJumpInput)
        {
            stateMachine.ChangeState(player.WallJumpState);
            return;
        }
    }
}
