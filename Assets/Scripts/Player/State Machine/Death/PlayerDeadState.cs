using UnityEngine;

public class PlayerDeadState : PlayerDeathState, IDeathState
{
    public PlayerDeadState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        player.FrameVelocity = Vector2.zero;
        player.Animator.Play(PlayerController.DeathAnim, -1, 0f);
    }

    public override void ExitState()
    {
        base.ExitState();
        player.InteractionBox.enabled = true;
    }

    public override void CheckForTransition()
    {
        // No. Unless... we later implement revive.
    }

    public override void PhysicsUpdate()
    {
        // No.
    }

    public override void OnAnimationEventTriggered(PlayerController.AnimationTriggerType triggerType)
    {
        if (triggerType == PlayerController.AnimationTriggerType.DeathComplete)
        {
            player.PromptRetry();
            return;
        }
    }
}
