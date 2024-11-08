using UnityEngine;

public class PlayerAirFlinchState : PlayerFlinchState, IAirState
{
    #region Air State

    private InnerAirState _airState;
    private InnerAirState AirState => _airState ??= new InnerAirState(player, stateMachine);

    public void HandleAirControl()
    {
        // Do nothing. Character cannot move.
    }

    public void HandleGravity() => AirState.HandleGravity();

    private class InnerAirState : PlayerAirState
    {
        public InnerAirState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
        {
        }
    }

    #endregion

    public PlayerAirFlinchState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        knockbackSpeed = player.Stats.AirFlinchKnockbackSpeed;
        knockbackDistance = player.Stats.AirFlinchKnockbackDistance;
        residualKnockbackSpeed = knockbackSpeed * player.Stats.AirFlinchResidualKnockbackSpeedRatio;
        base.EnterState();
        player.Animator.Play(PlayerController.AirFlinchAnim, -1, 0f);
    }

    protected override void StartKnockback()
    {
        player.FrameVelocity.y = player.Stats.AirFlinchKnockupSpeed;
    }

    public override void ExitState()
    {
        base.ExitState();
        player.Animator.Play(PlayerController.AirFlinchEndAnim, -1, 0f);
    }

    public override void CheckForTransition()
    {
        base.CheckForTransition();
        if (stateMachine.Transitioning) return;
        if (player.TimeFlinchStarted + player.Stats.AirFlinchDuration <= Time.time)
        {
            player.ReturnToNeutral();
            return;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (!player.BodyContacts.Ground) HandleGravity();
    }
}
