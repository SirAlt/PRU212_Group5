using UnityEngine;

public class PlayerDyingState : PlayerDeathState, IDeathState
{
    private bool _animCompleted;
    private float _safeguardTimer;

    public PlayerDyingState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();

        _animCompleted = false;
        _safeguardTimer = 0f;

        if (player.BodyContacts.Ground) player.FrameVelocity.x = 0f;

        player.InteractionBox.enabled = false;

        player.FX.StopAllEffects();
        player.Animator.Play(PlayerController.DyingAnim, -1, 0f);
    }

    public override void ExitState()
    {
        base.ExitState();
        ScreenShake.Instance.Shake(_safeguardTimer, 0.25f);
    }

    public override void CheckForTransition()
    {
        if (_animCompleted)
        {
            if (player.BodyContacts.Ground || _safeguardTimer >= player.Stats.TransitionToDeathSafeguardTimeLimit)
            {
                stateMachine.ChangeState(player.DeadState);
                return;
            }
            _safeguardTimer += Time.fixedDeltaTime;
        }
    }

    public override void PhysicsUpdate()
    {
        player.FrameVelocity.y = Mathf.MoveTowards(player.FrameVelocity.y, -1.0f * player.Stats.FallSpeedClamp, player.Stats.GravitationalAcceleration * Time.fixedDeltaTime);
    }

    public override void OnAnimationEventTriggered(PlayerController.AnimationTriggerType triggerType)
    {
        // Ignore universal triggers like 'Flinch' and 'DyingStart'.
        if (triggerType == PlayerController.AnimationTriggerType.DyingEnd)
        {
            _animCompleted = true;
            return;
        }
    }
}
