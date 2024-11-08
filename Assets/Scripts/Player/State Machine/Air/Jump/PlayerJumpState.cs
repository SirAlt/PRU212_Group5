using UnityEngine;

public abstract class PlayerJumpState : PlayerAirState, IJumpState
{
    #region Apex Modifier

    public virtual float ApexRatio => Mathf.InverseLerp(player.Stats.ApexThreshold, 0, Mathf.Abs(player.FrameVelocity.y));

    public virtual void ApplyApexModifier()
    {
        var apexRatio = ApexRatio;

        if (player.Input.Move.x != 0)
        {
            var apexVelocityBonus = player.Stats.ApexAccelerationBonus * apexRatio * Time.fixedDeltaTime;
            player.FrameVelocity.x += apexVelocityBonus * Mathf.Sign(player.Input.Move.x);
        }

        var apexAntiGravBonus = player.Stats.ApexAntiGravityBonus * apexRatio;
        gravity = Mathf.MoveTowards(gravity, 0, apexAntiGravBonus); // Improved logic. No longer allows "upward" gravity.
    }

    #endregion

    public bool Reentrant { get; set; }

    protected bool jumpEndedEarly;

    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        player.TimeJumpStarted = Time.time;
        player.Input.ClearJump();
        jumpEndedEarly = false;

        if (!Reentrant) ExecuteJump();
        Reentrant = false;
    }

    public virtual void ExecuteJump()
    {
    }

    public override void ExitState()
    {
        base.ExitState();
        jumpEndedEarly = false;
        Reentrant = false;
    }

    public override void CheckForTransition()
    {
        base.CheckForTransition();
        if (stateMachine.Transitioning && stateMachine.NextState is not PlayerAirSlashState) return;
        if (player.FrameVelocity.y <= 0)
        {
            stateMachine.ChangeState(player.JumpFallState);
            return;
        }
    }

    // TRACE: This state implements custom physics.
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    protected override void HandleCeilingHit()
    {
        if (player.TimeJumpStarted + player.Stats.InitialJumpPeriod > Time.time) return;
        base.HandleCeilingHit();
    }

    public override void HandleGravity()
    {
        gravity = player.Stats.GravitationalAcceleration;

        if (!jumpEndedEarly) CheckIfJumpEndedEarly();

        if (jumpEndedEarly && player.TimeJumpStarted + player.Stats.JumpCutoffPoint <= Time.time)
        {
            gravity *= player.Stats.JumpEndEarlyGravityModifier;
        }
        else if (player.TimeJumpStarted + player.Stats.InitialJumpPeriod > Time.time)
        {
            gravity *= player.Stats.InitialJumpGravityModifier;
            player.FrameVelocity.y += player.Stats.InitialJumpAcceleration * Time.fixedDeltaTime;
        }
        else
        {
            ApplyApexModifier();
        }

        player.FrameVelocity.y = Mathf.MoveTowards(player.FrameVelocity.y, -1.0f * player.Stats.FallSpeedClamp, gravity * Time.fixedDeltaTime);
    }

    protected virtual void CheckIfJumpEndedEarly()
    {
        if (!player.Input.JumpHeld
            && player.TimeJumpStarted + player.Stats.JumpEndEarlyWindow > Time.time)
        {
            jumpEndedEarly = true;
        }
    }
}
