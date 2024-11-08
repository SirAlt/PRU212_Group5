using UnityEngine;

public class PlayerDashSlashState : PlayerDashState, IAttackState
{
    public PlayerDashSlashState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();

        player.Input.ClearSlash();

        RemoveInvincibility();

        player.FrameVelocity.x = player.Stats.DashSlashMoveSpeed * Mathf.Sign(player.FrameVelocity.x);

        player.Animator.Play(PlayerController.DashSlashAnim, -1, 0f);
    }

    public override void ExitState()
    {
        base.ExitState();
        DeactivateAttack();
    }

    public override void CheckForTransition()
    {
        // PRIO: *WallJump* > *GroundJump* > *AirJump* > Run > Idle > NatFall
        if (player.Stats.CanJumpCancelDash && player.HasValidJumpInput)
        {
            if (player.BodyContacts.Ground || player.TimeLeftGround + player.Stats.CoyoteTime > Time.time)
            {
                stateMachine.ChangeState(player.GroundJumpState);
                return;
            }
            if (player.Abilities.WallJumpLearnt && player.BodyContacts.Wall)
            {
                stateMachine.ChangeState(player.WallJumpState);
                return;
            }
            if (player.AirDashCharges > 0)
            {
                stateMachine.ChangeState(player.AirJumpState);
                return;
            }
        }

        // Run > Idle > NatFall are executed upon attack animation finish.
    }

    public override void OnAnimationEventTriggered(PlayerController.AnimationTriggerType triggerType)
    {
        base.OnAnimationEventTriggered(triggerType);
        switch (triggerType)
        {
            case PlayerController.AnimationTriggerType.AttackActiveFramesStarted:
                ActivateAttack();
                break;
            case PlayerController.AnimationTriggerType.AttackActiveFramesEnded:
                DeactivateAttack();
                break;
            case PlayerController.AnimationTriggerType.AttackFinished:
                if (player.BodyContacts.Ground)
                    player.Land(immediate: true);
                else
                    stateMachine.ChangeState(player.NaturalFallState, immediate: true);
                break;
        }
    }

    public void ActivateAttack()
    {
        player.DashSlash.SetActive(true);
        player.TimeSlashActivated = Time.time;
    }

    public void DeactivateAttack() => player.DashSlash.SetActive(false);
}
