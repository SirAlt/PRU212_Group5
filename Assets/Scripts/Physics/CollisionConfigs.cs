using UnityEngine;

[CreateAssetMenu]
public class CollisionConfigs : ScriptableObject
{
    [Header("Collision")]
    [Tooltip("Layer(s) of terrain, solid objects, and other movement-limiting entities (such as invisible walls).")]
    public LayerMask ObstacleLayer;

    [Range(0f, 0.5f)]
    [Tooltip("Extra safety distance used when calculating collisions. Helps prevent moving too close and getting stuck inside terrain.")]
    public float CollisionOffset;


    [Header("Contacts")]
    [Tooltip("Layer(s) of objects considered solid ground.")]
    public LayerMask GroundLayer;

    [Tooltip("Layer(s) of objects considered solid ceilings.")]
    public LayerMask CeilingLayer;

    [Tooltip("Layer(s) of objects considered solid walls.")]
    public LayerMask WallLayer;

    [Range(0f, 0.5f)]
    [Tooltip("How far above solid objects the character is considered to be grounded.")]
    public float GroundTouchDistance;

    [Range(0f, 0.5f)]
    [Tooltip("How far below solid objects the character is considered to be in contact with it.")]
    public float CeilingTouchDistance;

    [Range(0f, 0.5f)]
    [Tooltip("How far away from solid object the character is considered to be in contact with it. Useful for abilities like wallslide and walljump.")]
    public float WallTouchDistance;


    [Header("Slope")]
    [Range(0f, 90f)]
    [Tooltip("Maximum angle of inclination at which a surface considered to be a slope. Beyond this the surface considered a wall.")]
    public float MaxSlopeAngle;


    [Header("One-way Platform")]
    [Tooltip("Layer(s) of one-way platforms. One-way platforms only register collisions from above.")]
    public LayerMask OneWayPlatformLayer;
}
