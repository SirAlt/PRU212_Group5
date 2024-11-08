using UnityEngine;

// FIXME: This whole state is a messy pile of hacks. It holds an underlying air state and uses that for transition checks and physics,
// stopping/reverting any transition/action that it doesn't want. This is unmaintainable.
// The proper implementation would be to use a sub-state machine.
// However, that means duplicated code. If only there were a way to clone the preceeding air state, assign it to this state's internal state machine,
// and override some (but not all) of its methods.
// - <Jump> states should skip applying jump force on re-entrance.
// - Differences in transition logic: Dash -> Needs cancel enabled; Jump -> Needs cancel enabled; Attack -> Needs combo enabled.
// - In physics update, lock facing according to settings.

public class PlayerAirSlashState : PlayerAirState, IAttackState
{
    private PlayerAirState _underlyingAirState;

    private float _cachedTimeJumpWasPressed = Mathf.NegativeInfinity;
    private float _cachedTimeDashWasPressed = Mathf.NegativeInfinity;
    private float _cachedTimeSlashWasPressed = Mathf.NegativeInfinity;

    public PlayerAirSlashState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();

        if (stateMachine.PrevState is not PlayerAirState preceedingAirState)
        {
            Debug.LogError($"Illegal transition. Previous state was not a pure [ Air ] state.");
            stateMachine.Reset();
            return;
        }
        _underlyingAirState = preceedingAirState;

        player.Input.ClearSlash();
        player.Animator.Play(PlayerController.AirSlashAnim, -1, 0f);
    }

    public override void ExitState()
    {
        base.ExitState();
        DeactivateAttack();

        // Warning! High levels of ugliness.
        if (_cachedTimeJumpWasPressed > 0) player.Input.PressJump(_cachedTimeJumpWasPressed);
        if (_cachedTimeDashWasPressed > 0) player.Input.PressDash(_cachedTimeDashWasPressed);
        if (_cachedTimeSlashWasPressed > 0) player.Input.PressSlash(_cachedTimeSlashWasPressed);
    }

    // FIXME: Extremely hacky.
    public override void CheckForTransition()
    {
        // PRIO: Run > Idle > *AirDash* > *WallJump* > WallSlide > *(Coyote)GroundJump* > *AirJump*

        // Warning!! Extreme levels of repulsiveness.
        if (!player.Stats.CanJumpCancelAttack && player.HasJumpInput)
        {
            _cachedTimeJumpWasPressed = player.Input.TimeJumpWasPressed;
            player.Input.ClearJump();
        }
        if (!player.Stats.CanDashCancelAttack && player.HasDashInput)
        {
            _cachedTimeDashWasPressed = player.Input.TimeDashWasPressed;
            player.Input.ClearDash();
        }
        if (player.HasSlashInput)
        {
            _cachedTimeSlashWasPressed = player.Input.TimeSlashWasPressed;
            player.Input.ClearSlash();
        }

        // Run > Idle > AirDash > WallJump > WallSlide > (Coyote)GroundJump > AirJump > AirSlash > <Fall>
        _underlyingAirState.CheckForTransition();

        if (stateMachine.Transitioning)
        {
            if (stateMachine.NextState is PlayerGroundState or PlayerWallState) return;
            if (stateMachine.NextState is PlayerFallState fallState)
            {
                _underlyingAirState = fallState;
                stateMachine.StopTransition();
                return;
            }
        }
    }

    // FIXME: Extremely hacky.
    public override void PhysicsUpdate()
    {
        var cachedIsFacingRight = player.IsFacingRight;

        _underlyingAirState.PhysicsUpdate();

        // Warning!!! Unfathomable levels of hideousness.
        if (!player.Stats.CanTurnDuringJumpAttack && (cachedIsFacingRight != player.IsFacingRight))
        {
            if (cachedIsFacingRight) player.FaceRight();
            else player.FaceLeft();
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
                // FIXME: Would be better if we could override the enter methods instead.
                if (_underlyingAirState is PlayerJumpState jumpState) jumpState.Reentrant = true;
                // FIXME: Should somehow pass the underlying jump state.
                stateMachine.ChangeState(_underlyingAirState);
                break;
        }
    }

    public void ActivateAttack()
    {
        player.AirSlash.SetActive(true);
        player.TimeSlashActivated = Time.time;
    }

    public void DeactivateAttack() => player.AirSlash.SetActive(false);
}
