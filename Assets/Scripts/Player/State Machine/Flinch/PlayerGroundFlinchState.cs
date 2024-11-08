using UnityEngine;

public class PlayerGroundFlinchState : PlayerFlinchState, IGroundState
{
    public PlayerGroundFlinchState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        knockbackSpeed = player.Stats.GroundFlinchKnockbackSpeed;
        knockbackDistance = player.Stats.GroundFlinchKnockbackDistance;
        residualKnockbackSpeed = 0;
        base.EnterState();
        player.Animator.Play(PlayerController.GroundFlinchAnim, -1, 0f);
    }

    public override void ExitState()
    {
        base.ExitState();
        player.Animator.Play(PlayerController.GroundFlinchEndAnim, -1, 0f);
    }

    public override void CheckForTransition()
    {
        base.CheckForTransition();
        if (stateMachine.Transitioning) return;
        if (player.TimeFlinchStarted + player.Stats.GroundFlinchDuration <= Time.time)
        {
            player.ReturnToNeutral();
            return;
        }
    }
}
