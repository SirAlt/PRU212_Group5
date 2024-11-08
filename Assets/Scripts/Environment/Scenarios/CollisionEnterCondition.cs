using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollisionEnterCondition : Condition
{
    [SerializeField] private List<string> triggeringTags = new();
    [SerializeField] private LayerMask triggeringLayer;
    [SerializeField] private UseColliderType useColliderTypes;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!useColliderTypes.HasFlag(UseColliderType.NonTrigger)) return;

        if (triggeringTags.Contains(collision.gameObject.tag)
            || (triggeringLayer & (1 << collision.gameObject.layer)) != 0)
        {
            NotifyConditionFulfilled();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!useColliderTypes.HasFlag(UseColliderType.Trigger)) return;

        if (triggeringTags.Contains(collision.gameObject.tag)
            || (triggeringLayer & (1 << collision.gameObject.layer)) != 0)
        {
            NotifyConditionFulfilled();
        }
    }

    [Flags]
    private enum UseColliderType
    {
        NonTrigger = 1,
        Trigger = 2,
    }
}
