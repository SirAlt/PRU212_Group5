using UnityEngine;

public class PlayerRunSlashState : PlayerRunState, IAttackState
{
    protected int noMoveInputFrameCount;

    public PlayerRunSlashState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        player.Input.ClearSlash();
        noMoveInputFrameCount = 0;
        player.Animator.Play(PlayerController.RunSlashAnim, -1, 0f);
    }

    public override void ExitState()
    {
        base.ExitState();
        DeactivateAttack();
        noMoveInputFrameCount = 0;
    }

    public override void CheckForTransition()
    {
        // Inheriting from a concrete state ([ Run ]) that can transition to us has caused a great deal of headache,
        // including having to check for and prevent self-transition, which is not allowed for this [ Run Slash ] state.
        // > [ Run ] checks for attack input to transition to [ Run/Ground Slash ]. We do not. We ended up always having to stop those.
        // > [ Run ]'s attack calls (used to) overrule and hide [ Ground ]'s jump calls from us.
        // > We need to keep count of the number of consecutive frames where we've received no move input. We previously relied on checking for [ Run ] -> [ Idle ] calls.
        //   But these calls can be hidden by the wretched attack calls.

        if (player.Input.Move.x == 0)
            ++noMoveInputFrameCount;
        else
            noMoveInputFrameCount = 0;

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

        if (player.Stats.CancelAttackMoveWhenNoLongerMoving
            && noMoveInputFrameCount > player.Stats.AttackMoveCancelTolerance)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }
    }

    // TRACE: This state implements custom physics.
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    protected override void UpdateFacing()
    {
        if (player.Stats.CanTurnDuringAttackMove) base.UpdateFacing();
    }

    protected override void HandleGroundControl()
    {
        if (player.Input.Move.x == 0
            || (!player.Stats.CanTurnDuringAttackMove && player.IsTurningAround))
        {
            player.FrameVelocity.x = Mathf.MoveTowards(player.FrameVelocity.x, 0, player.Stats.GroundDeceleration * Time.fixedDeltaTime);
        }
        else
        {
            base.HandleGroundControl();
        }
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
        player.RunSlash.SetActive(true);
        player.TimeSlashActivated = Time.time;
    }

    public void DeactivateAttack() => player.RunSlash.SetActive(false);
}
