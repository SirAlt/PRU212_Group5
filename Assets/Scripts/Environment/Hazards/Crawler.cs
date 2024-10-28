using System.Collections.Generic;
using UnityEngine;

public class Crawler : MonoBehaviour
{
    [SerializeField] private LayerMask terrainLayers;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float flipPointReachedDistance = 0.15f;
    [SerializeField] private List<Transform> flipPoints = new();

    [Header("Ground detection")]
    [SerializeField] private Transform groundChecker;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float groundTurnNudgeRight;
    [SerializeField] private float groundTurnNudgeDown;

    [Header("Wall detection")]
    [SerializeField] private Transform wallChecker;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float wallTurnNudgeRight;
    [SerializeField] private float wallTurnNudgeUp;

    private void FixedUpdate()
    {
        CheckFlipPoints();
        CheckFooting();
        Move();
    }

    private void CheckFlipPoints()
    {
        foreach (Transform flipPoint in flipPoints)
        {
            if (Vector2.Distance(transform.position, flipPoint.position) <= flipPointReachedDistance)
            {
                transform.Rotate(0f, 180f, 0f);
                return;
            }
        }
    }

    private void CheckFooting()
    {
        // Note the negation for ground-turn.
        bool groundTurn = !Physics2D.Raycast(groundChecker.position, -1.0f * transform.up, groundCheckDistance, terrainLayers);
        bool wallTurn = Physics2D.Raycast(wallChecker.position, transform.right, wallCheckDistance, terrainLayers);

        // If we're approaching a small gap with a wall on the other side, both ground-turn and wall-turn will be true.
        // In such cases, prioritize ground.
        if (groundTurn)
        {
            transform.position += groundTurnNudgeRight * transform.right;
            transform.position -= groundTurnNudgeDown * transform.up;
            transform.Rotate(0f, 0f, -90f);
        }
        else if (wallTurn)
        {
            transform.position += wallTurnNudgeRight * transform.right;
            transform.position += wallTurnNudgeUp * transform.up;
            transform.Rotate(0f, 0f, 90f);
        }
    }

    private void Move()
    {
        transform.Translate(moveSpeed * Time.fixedDeltaTime * Vector2.right);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundChecker.position, groundChecker.position + groundCheckDistance * -1.0f * transform.up);
        Gizmos.DrawLine(wallChecker.position, wallChecker.position + wallCheckDistance * transform.right);
    }
#endif
}
