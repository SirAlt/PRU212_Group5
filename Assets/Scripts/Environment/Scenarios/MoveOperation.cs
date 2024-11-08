using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOperation : Operation
{
    [SerializeField] private GameObject target;
    [SerializeField] private List<Transform> destinations = new();
    [SerializeField] private List<float> speeds = new();
    [SerializeField] private List<float> waits = new();

    public override void Execute()
    {
        StartCoroutine(nameof(MoveToNextDestination), 0);
    }

    private IEnumerator MoveToNextDestination(int idx)
    {
        yield return new WaitForSeconds(waits[idx]);

        var curDest = destinations[idx].position;
        if (speeds[idx] <= 0f)
        {
            TeleportTarget(curDest);
        }
        else
        {
            var curSpeed = speeds[idx];
            while (Vector2.Distance(target.transform.position, destinations[idx].position) > 0.01f)
            {
                var nextPos = Vector3.MoveTowards(target.transform.position, curDest, curSpeed * Time.fixedDeltaTime);
                TeleportTarget(nextPos);
                yield return new WaitForFixedUpdate();
            }
        }

        if (++idx < destinations.Count)
            StartCoroutine(nameof(MoveToNextDestination), idx);
    }

    private void TeleportTarget(Vector3 position)
    {
        target.transform.position = position;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var destCount = destinations.Count;
        var waitCount = waits.Count;
        var speedCount = speeds.Count;

        if (destCount == 0 || waitCount == 0 || speedCount == 0)
        {
            Debug.LogWarning($"At least one of destination, speed, or wait lists of {nameof(MoveOperation)} on {gameObject} is empty.", gameObject);
        }
        if (destCount != waitCount || destCount != speedCount)
        {
            Debug.LogWarning($"Mismatching number of destinations, speeds, or waits of {nameof(MoveOperation)} on {gameObject}.", gameObject);
        }
    }
#endif
}
