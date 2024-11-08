using UnityEngine;

public class PlayerIdleState : PlayerGroundState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        switch (stateMachine.PrevState)
        {
            case PlayerGroundDashState:
                player.Animator.Play(PlayerController.GroundDashEndAnim, -1, 0f);
                break;
            case PlayerAirDashState:
                player.Animator.Play(PlayerController.AirDashEndAnim, -1, 0f);
                break;
            case PlayerGroundFlinchState:
                player.Animator.Play(PlayerController.GroundFlinchEndAnim, -1, 0f);
                break;
            case PlayerAirFlinchState:
                player.Animator.Play(PlayerController.AirFlinchEndAnim, -1, 0f);
                break;
            default:
                player.Animator.Play(PlayerController.IdleAnim, -1, 0f);
                break;
        }
    }

    public override void CheckForTransition()
    {
        base.CheckForTransition();
        if (stateMachine.Transitioning) return;
        if (player.Input.Move.x != 0)
        {
            stateMachine.ChangeState(player.RunState);
            return;
        }
        if (player.HasValidSlashInput)
        {
            stateMachine.ChangeState(player.StandSlashState);
            return;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.FrameVelocity.x = Mathf.MoveTowards(player.FrameVelocity.x, 0, player.Stats.GroundDeceleration * Time.fixedDeltaTime);
    }
}
