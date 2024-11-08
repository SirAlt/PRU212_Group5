using UnityEngine;

public class PlayerRunState : PlayerGroundState
{
    public PlayerRunState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();

        var offset = 0f;
        if (stateMachine.PrevState is not IGroundState) offset = 0.35f;
        player.Animator.Play(PlayerController.RunAnim, -1, offset);
    }

    public override void CheckForTransition()
    {
        base.CheckForTransition();

        // Ugly. Since we're concrete and have derived classes, when called by them, we run into issues where
        // we try to make a transition from that derived state into itself or another derived state.
        // The solution is then to yield control if a derived state is the current state.
        //
        // Welp. We've settled for having derived states reimplement all transition logic themselves.

        //if (stateMachine.CurrentState != _player.RunState) return;

        if (stateMachine.Transitioning) return;
        if (player.Input.Move.x == 0)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }
        if (player.HasValidSlashInput)
        {
            if (player.Stats.CanAttackMove)
                stateMachine.ChangeState(player.RunSlashState);
            else
                stateMachine.ChangeState(player.StandSlashState);
            return;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        HandleGroundControl();
    }

    protected virtual void HandleGroundControl()
    {
        player.FrameVelocity.x = Mathf.MoveTowards(player.FrameVelocity.x,
                                   player.Stats.MaxGroundSpeed * Mathf.Sign(player.Input.Move.x),
                                   player.Stats.GroundAcceleration * Time.fixedDeltaTime);
    }
}
