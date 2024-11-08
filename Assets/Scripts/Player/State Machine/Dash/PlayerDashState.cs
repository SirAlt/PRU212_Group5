using UnityEngine;

public class PlayerDashState : PlayerState, IDashState
{
    protected Vector2 dashVector;
    protected bool dashEndedEarly;
    protected bool invincibilityGranted;

    private Vector2 _cachedCollisionBoxSize;
    private Vector2 _cachedHurtboxSize;

    public PlayerDashState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();

        player.TimeDashStarted = Time.time;
        player.Input.ClearDash();
        dashEndedEarly = false;

        ExecuteDash();
        ModifyHitboxes();
        if (player.Abilities.InvincibleDash) GrantInvincibility();
    }

    public virtual void ExecuteDash()
    {
        var dashDirection = GetDashDirection();
        dashVector = new Vector2(dashDirection * player.Stats.DashSpeed, 0);
        player.FrameVelocity = dashVector;
    }

    protected virtual float GetDashDirection()
    {
        if (player.Input.Move.x != 0)
        {
            UpdateFacing();
            return Mathf.Sign(player.Input.Move.x);
        }
        else
        {
            return player.IsFacingRight ? 1.0f : -1.0f;
        }
    }

    protected virtual void ModifyHitboxes()
    {
        var colBoxSize = _cachedCollisionBoxSize = player.CollisionBox.size;
        var colBoxScale = player.Stats.DashCollisionBoxScale;
        player.CollisionBox.size = new Vector2(colBoxSize.x * colBoxScale.x, colBoxSize.y * colBoxScale.y);


        var hurtboxSize = _cachedHurtboxSize = player.Hurtbox.Collider.size;
        var hurtboxScale = player.Stats.DashHurtboxScale;
        player.Hurtbox.Collider.size = new Vector2(hurtboxSize.x * hurtboxScale.x, hurtboxSize.y * hurtboxScale.y);
    }

    protected virtual void GrantInvincibility()
    {
        invincibilityGranted = true;
        player.Hurtbox.GainInvincibility();
    }

    public override void ExitState()
    {
        base.ExitState();

        player.TimeDashEnded = Time.time;
        dashEndedEarly = false;

        RestoreHitboxes();
        if (invincibilityGranted) RemoveInvincibility();
    }

    protected virtual void RestoreHitboxes()
    {
        player.CollisionBox.size = _cachedCollisionBoxSize;
        player.Hurtbox.Collider.size = _cachedHurtboxSize;
    }

    protected virtual void RemoveInvincibility()
    {
        invincibilityGranted = false;
        player.Hurtbox.RemoveInvincibility();
    }

    public override void CheckForTransition()
    {
        base.CheckForTransition();
        if (stateMachine.Transitioning) return;

        if (player.Stats.CanJumpCancelDash && player.HasValidJumpInput)
        {
            if (player.BodyContacts.Ground || player.TimeLeftGround + player.Stats.CoyoteTime > Time.time)
            {
                stateMachine.ChangeState(player.GroundJumpState);
                return;
            }
            if (player.AirJumpCharges > 0)
            {
                stateMachine.ChangeState(player.AirJumpState);
                return;
            }
        }

        if (player.HasValidSlashInput)
        {
            stateMachine.ChangeState(player.DashSlashState);
            return;
        }

        if (!dashEndedEarly) CheckIfDashEndedEarly();
        if (dashEndedEarly || player.TimeDashStarted + player.Stats.DashDuration <= Time.time)
        {
            player.ReturnToNeutral();
            return;
        }
    }

    protected virtual void CheckIfDashEndedEarly()
    {
        if (!player.Input.DashHeld
            && player.TimeDashStarted + player.Stats.DashEndEarlyWindow > Time.time)
        {
            dashEndedEarly = true;
        }
    }

    public override void PhysicsUpdate()
    {
        // Do nothing. Maintain velocity.
    }
}
