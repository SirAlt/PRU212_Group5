using UnityEngine;

public class PlayerFlinchState : PlayerState, IFlinchState
{
    protected float knockbackSpeed;
    protected float knockbackDistance;
    protected float residualKnockbackSpeed;

    protected float distanceKnockedBack;

    protected PlayerFlinchState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        player.TimeFlinchStarted = Time.time;
        distanceKnockedBack = 0f;
        StartKnockback();
    }

    protected virtual void StartKnockback()
    {
        player.FrameVelocity.y = 0f;
    }

    public override void ExitState()
    {
        base.ExitState();
        distanceKnockedBack = 0f;
    }

    public override void CheckForTransition()
    {
        base.CheckForTransition();
        if (stateMachine.Transitioning) return;
        if (player.Stats.CanJumpRecoverFromFlinch
            && (player.HasValidJumpInput || (player.Input.JumpHeld && !player.JumpOnCooldown))
            && (player.TimeFlinchStarted + player.Stats.FlinchRecoveryCutoffTime <= Time.time))
        {
            if (player.Input.Move.y < 0) goto GET_UP;
            if (player.BodyContacts.Ground)
            {
                stateMachine.ChangeState(player.GroundJumpState);
                return;
            }
            if (player.AirJumpCharges > 0)
            {
                stateMachine.ChangeState(player.AirJumpState);
                return;
            }
        GET_UP:
            stateMachine.ChangeState(player.GetUpState);
            return;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        UpdateKnockback();
    }

    protected virtual void UpdateKnockback()
    {
        float frameKnockbackSpeed;
        if (distanceKnockedBack >= knockbackDistance)
        {
            frameKnockbackSpeed = residualKnockbackSpeed;
        }
        else if (knockbackSpeed * Time.fixedDeltaTime <= (knockbackDistance - distanceKnockedBack))
        {
            frameKnockbackSpeed = knockbackSpeed;
        }
        else
        {
            frameKnockbackSpeed = (knockbackDistance - distanceKnockedBack) / Time.fixedDeltaTime;
        }
        player.FrameVelocity.x = frameKnockbackSpeed * Mathf.Sign(player.LastHitDirection.x);
        distanceKnockedBack += frameKnockbackSpeed * Time.fixedDeltaTime;
    }

    protected override void UpdateFacing()
    {
        // Do nothing. Character cannot turn.
    }
}
