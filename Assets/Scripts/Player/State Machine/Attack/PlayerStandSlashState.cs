using UnityEngine;

public class PlayerStandSlashState : PlayerGroundState, IAttackState
{
    public PlayerStandSlashState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        player.Input.ClearSlash();
        player.Animator.Play(PlayerController.StandSlashAnim, -1, 0f);
    }

    public override void ExitState()
    {
        base.ExitState();
        DeactivateAttack();
    }

    public override void CheckForTransition()
    {
        // Reimplement transition logic.
        if (!player.BodyContacts.Ground)
        {
            stateMachine.ChangeState(player.NaturalFallState);
            return;
        }
        if (player.Stats.CanDashCancelAttack && player.HasValidDashInput)
        {
            stateMachine.ChangeState(player.GroundDashState);
            return;
        }
        if (player.Stats.CanJumpCancelAttack && player.HasValidJumpInput)
        {
            stateMachine.ChangeState(player.GroundJumpState);
            return;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.FrameVelocity.x = Mathf.MoveTowards(player.FrameVelocity.x, 0, player.Stats.GroundDeceleration * Time.fixedDeltaTime);
    }

    protected override void UpdateFacing()
    {
        if (player.Stats.CanTurnDuringStandingAttack) base.UpdateFacing();
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
                player.Land(immediate: true);
                break;
        }
    }

    public void ActivateAttack()
    {
        player.StandSlash.SetActive(true);
        player.TimeSlashActivated = Time.time;
    }

    public void DeactivateAttack() => player.StandSlash.SetActive(false);
}
