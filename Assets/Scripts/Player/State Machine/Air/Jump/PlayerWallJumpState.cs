using UnityEngine;

public class PlayerWallJumpState : PlayerJumpState
{
    protected Vector2 jumpVector;

    public PlayerWallJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        player.Animator.Play(PlayerController.WallJumpAnim, -1, 0f);
    }

    public override void ExecuteJump()
    {
        float direction = 0;
        // Prioritize jumping left-to-right
        if (player.BodyContacts.WallLeft) { direction = -1; player.FaceRight(); }
        else if (player.BodyContacts.WallRight) { direction = 1; player.FaceLeft(); }
        if (direction == 0)
        {
            if (player.LastWallContactSide < 0) { direction = -1; player.FaceRight(); }
            else if (player.LastWallContactSide > 0) { direction = 1; player.FaceLeft(); }
        }
        // This is basically an argument [ Natural Fall ] passes to us to specify which wall it is that we're to "coyote jump" off of. Ergo, we always have to unset it.
        player.LastWallContactSide = 0;
        if (direction == 0)
        {
            Debug.LogError("Illegal transition. Character is not in contact with any walls.");
            stateMachine.Reset();
            return;
        }

        var jumpPower = player.Abilities.WallJumpPower > 0 ? player.Abilities.WallJumpPower : player.Stats.JumpPower;
        var jumpAngle = player.Abilities.WallJumpAngle * direction;
        var jumpDirection = Quaternion.Euler(0, 0, jumpAngle) * Vector2.up;
        jumpVector = jumpPower * jumpDirection;
        player.FrameVelocity = jumpVector;
    }

    public override void PhysicsUpdate()
    {
        if (!jumpEndedEarly) CheckIfJumpEndedEarly();

        var pastKickAwayPeriod =
            player.BodyContacts.Ceiling
            || (jumpEndedEarly && player.TimeJumpStarted + player.Stats.WallJumpCutoffPoint <= Time.time)
            || player.TimeJumpStarted + player.Stats.WallJumpInitialPeriod <= Time.time;
        if (pastKickAwayPeriod)
        {
            base.PhysicsUpdate();
        }
        else
        {
            UpdateFacing();
            HandleCeilingHit();
        }
    }
}
