using UnityEngine;

public class PlayerDropThroughFallState : PlayerNaturalFallState
{
    public PlayerDropThroughFallState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();

        player.TimeDropThroughStarted = Time.time;
        player.Input.ClearJump();

        player.BodyContacts.DropThrough = true;
        player.Mover.RemoveLayersX(player.Mover.Configurations.OneWayPlatformLayer);
        player.Mover.RemoveLayersY(player.Mover.Configurations.OneWayPlatformLayer);
    }

    public override void ExitState()
    {
        base.ExitState();
        player.BodyContacts.DropThrough = false;
        player.Mover.AddLayersX(player.Mover.Configurations.OneWayPlatformLayer);
        player.Mover.AddLayersY(player.Mover.Configurations.OneWayPlatformLayer);
    }

    public override void CheckForTransition()
    {
        base.CheckForTransition();
        if (/*player.BodyContacts.ThroughOneWayPlatform ||*/ player.TimeDropThroughStarted + player.Stats.DropThroughInitialGracePeriod > Time.time) return;
        if (player.Stats.CanMaintainDropThrough && (player.Input.Move.y < 0 || player.Input.JumpHeld)) return;
        stateMachine.ChangeState(player.NaturalFallState);
        return;
    }
}
