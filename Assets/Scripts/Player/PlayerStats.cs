using UnityEngine;

[CreateAssetMenu]
public class PlayerStats : ScriptableObject
{
    [Header("Ground Movement")]
    [Tooltip("Maximum horizontal movement speed on the ground.")]
    public float MaxGroundSpeed;

    [Tooltip("How fast the character gains horizontal movement speed.")]
    public float GroundAcceleration;

    [Tooltip("How fast the character's horizontal movement stops while on the ground.")]
    public float GroundDeceleration;


    [Header("Air Movement")]
    [Tooltip("Maximum horizontal movement speed in the air.")]
    public float MaxAirSpeed;

    [Tooltip("How fast the character gains horizontal movement speed.")]
    public float AirAcceleration;

    [Tooltip("How fast the character's horizontal movement stops while in the air.")]
    public float AirDeceleration;


    [Header("Gravity")]
    [Tooltip("Downward acceleration when in the air.")]
    public float GravitationalAcceleration;

    [Tooltip("Terminal velocity.")]
    public float FallSpeedClamp;


    [Header("Jump")]
    [Tooltip("Velocity gained instantly at the start of a jump.")]
    public float JumpPower;

    [Tooltip("Continual upward acceleration applied during the beginning portion of a jump.")]
    public float InitialJumpAcceleration;

    [Range(0f, 1f)]
    [Tooltip("Gravity strength during the beginning portion of a jump. Helps make the jump feel less floaty.")]
    public float InitialJumpGravityModifier;

    [Tooltip("The amount of time upward acceleration and modified gravity is in effect.")]
    public float InitialJumpPeriod;


    [Header("Variable Jump Height")]
    [Tooltip("The gravity multiplier added when jump is released early.")]
    public float JumpEndEarlyGravityModifier = 3;

    [Tooltip("The smallest amount that must pass before jump cut has any effect.")]
    public float JumpCutoffPoint;

    [Tooltip("The time window for cutting a jump short. Past this point, the jump will reach max height even if the jump key is not held.")]
    public float JumpEndEarlyWindow;


    [Header("Input")]
    [Tooltip("The amount of time a jump is buffered. This allows jump input before actually hitting the ground.")]
    public float JumpBuffer;

    [Tooltip("The amount of time jump input is ignored after executing a jump. Prevents accidental jumps caused by button mashing.")]
    public float JumpCooldown;


    [Header("Coyote Time")]
    [Tooltip("The time before coyote jump becomes unusable. Coyote jump allows jump to execute even after leaving a ledge.")]
    public float CoyoteTime;

    [Tooltip("Similar to coyote time, but applies to wall jumps after leaving .")]
    public float WallCoyoteTime;


    [Header("Apex Modifier")]
    [Tooltip("Vertical speed below which apex modifier activates. The lower, the larger the apex modifier window.")]
    public float ApexThreshold;

    [Tooltip("Additional horizontal acceleration while near jump apex.")]
    public float ApexAccelerationBonus;

    [Tooltip("Counter-gravity applied while apex modifer is in effect.")]
    public float ApexAntiGravityBonus;


    [Header("Slope")]
    [Tooltip("Constant downward velocity applied while on the ground. Helps on slopes.")]
    public float GroundingVelocity;


    [Header("Drop Through")]
    [Tooltip("The grace period during which the drop-through state will not expire. Helps the character fall through platforms in cases of high collision offset.")]
    public float DropThroughInitialGracePeriod;

    [Tooltip("Whether the character can drop through multiple one-way platforms consecutively. Requires holding down the drop-through input.")]
    public bool CanMaintainDropThrough;


    [Header("Wall Jump")]
    [Tooltip("The amount of time the character is locked in animation at the start of a wall jump.")]
    public float WallJumpCutoffPoint;

    [Tooltip("Length of the period of time where the character strictly follows a wall jump's path, ignoring gravity. Can be interrupted by ending the jump early.")]
    public float WallJumpInitialPeriod;


    [Header("Dash")]
    [Tooltip("The speed at which the character moves during a dash.")]
    public float DashSpeed;

    [Tooltip("The dimen in time of a dash.")]
    public float DashDuration;

    [Tooltip("The amount of time a dash is buffered.")]
    public float DashBuffer;

    [Tooltip("The time window for cutting a dash short. Past this point, the dash will go full distance even if the dash key is released.")]
    public float DashEndEarlyWindow;

    [Tooltip("Whether the character can interrupt a dash by jumping.")]
    public bool CanJumpCancelDash;

    [Tooltip("Scaling applied to the character's collision box during a dash.")]
    public Vector2 DashCollisionBoxScale;

    [Tooltip("Scaling applied to the character's Collider during a dash.")]
    public Vector2 DashHurtboxScale;


    [Header("Attack")]
    [Tooltip("The amount of time a slash is buffered.")]
    public float SlashBuffer;

    [Tooltip("Whether the character can interrupt attack animations by jumping.")]
    public bool CanJumpCancelAttack;

    [Tooltip("Whether the character can interrupt attack animations by dashing.")]
    public bool CanDashCancelAttack;

    [Tooltip("Enables an alternative version of ground attack that maintains ground movement.")]
    public bool CanAttackMove;

    [Tooltip("If enabled, attack-move will canel if the character stops moving mid-execution.")]
    public bool CancelAttackMoveWhenNoLongerMoving;

    [Tooltip("The number of consecutive frames the character must receive no movement input before an attack-move is cancelled. Has no effect if attack-move cannot be cancelled.")]
    public int AttackMoveCancelTolerance;

    [Tooltip("Allows the character to turn and redirect ground standing attacks.")]
    public bool CanTurnDuringStandingAttack;

    [Tooltip("Allows the character to turn and redirect ground moving attacks.")]
    public bool CanTurnDuringAttackMove;

    [Tooltip("Allows the character to turn and redirect air jumping attacks.")]
    public bool CanTurnDuringJumpAttack;

    [Tooltip("Horizontal speed while executing dash attacks.")]
    public float DashSlashMoveSpeed;


    [Header("Flinch")]
    [Tooltip("The amount of time the character is unable to act after getting hit on the ground.")]
    public float GroundFlinchDuration;

    [Tooltip("The distance the character is knocked back during ground flinch.")]
    public float GroundFlinchKnockbackDistance;

    [Tooltip("The speed at which the character is knocked back during ground flinch.")]
    public float GroundFlinchKnockbackSpeed;

    [Tooltip("The amount of time the character is unable to act after getting hit in the air.")]
    public float AirFlinchDuration;

    [Tooltip("The distance the character is knocked back during air flinch.")]
    public float AirFlinchKnockbackDistance;

    [Tooltip("The speed at which the character is knocked back during air flinch.")]
    public float AirFlinchKnockbackSpeed;

    [Tooltip("Upward force applied to the character at the start of an air flinch.")]
    public float AirFlinchKnockupSpeed;

    [Tooltip("Proportion of knockback speed still in effect after reaching knockback distance."), Range(0f, 1f)]
    public float AirFlinchResidualKnockbackSpeedRatio;

    [Tooltip("Whether the character can jump to recover from a flinch early.")]
    public bool CanJumpRecoverFromFlinch;

    [Tooltip("The amount of time during a flinch before jump recovery becomes available.")]
    public float FlinchRecoveryCutoffTime;

    [Tooltip("The number of frames a get-up action takes.")]
    public int GetUpFrameCount;

    [Tooltip("Whether the character is invincible during get-up.")]
    public bool InvincibleGetUp;


    [Header("Post-damage Invincibility")]
    [Tooltip("How long the invinciblity the character gets after taking damage lasts.")]
    public float PostDamageInvincibilityDuration;

    [Tooltip("How fast the character flickers during certain types of invinciblity.")]
    public float InvincibilityFlickerInterval;


    [Header("Death")]
    [Tooltip("Normally the character would need to land to complete the death process. This sets a safety time limit in case of infinite fall.")]
    public float TransitionToDeathSafeguardTimeLimit;


    [Header("Respawn")]
    [Tooltip("The amount of time to wait, after completing death animation, to respawn at the last checkpoint.")]
    public float RespawnDelay;

    [Tooltip("How long the invinciblity the character gets right after respawning lasts.")]
    public float PostRespawnInvincibilityDuration;
}
