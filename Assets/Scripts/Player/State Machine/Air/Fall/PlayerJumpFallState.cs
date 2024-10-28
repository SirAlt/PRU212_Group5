using Unity.VisualScripting;
using UnityEngine;

public class PlayerJumpFallState : PlayerFallState, IJumpState
{
    #region Jump State

    private readonly IJumpState _defaultJumpState;

    private IJumpState _jumpState;

    // This doesn't need to exist, but it does because [ Air Slash ] nabs <Air> states for their PhysicsUpdate()
    // without calling their EnterState() or ExitState() methods. See there for more details.
    private IJumpState JumpState => _jumpState ??= _defaultJumpState; 

    public float ApexRatio => JumpState.ApexRatio;

    public void ApplyApexModifier() => JumpState.ApplyApexModifier();

    private class DefaultJumpState : PlayerJumpState
    {
        public DefaultJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
        {
        }
    }

    #endregion

    public PlayerJumpFallState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
        _defaultJumpState = new DefaultJumpState(player, stateMachine);
    }

    public override void EnterState()
    {
        base.EnterState();

        if (stateMachine.PrevState is not PlayerJumpState prevJumpState)
        {
            //Debug.Log($"Previous state was not a [ Jump ] state. Using defaults for jump state logics.", gameObject);
            _jumpState = _defaultJumpState;
        }
        else
        {
            _jumpState = prevJumpState;
        }

        if (stateMachine.PrevState is not PlayerFallState)
            player.Animator.Play(PlayerController.JumpFallAnim, -1, 0f);
    }

    // TRACE: This state implements custom physics.
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void HandleGravity()
    {
        gravity = player.Stats.GravitationalAcceleration;
        ApplyApexModifier();
        player.FrameVelocity.y = Mathf.MoveTowards(player.FrameVelocity.y, -1.0f * player.Stats.FallSpeedClamp, gravity * Time.fixedDeltaTime);
    }

    public void ExecuteJump()
    {
        // Do nothing.
    }
}
