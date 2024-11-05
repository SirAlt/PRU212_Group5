using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour, ITriggerable
{
    [SerializeField] private GameObject spawnPrefab;
    [SerializeField] private Transform parent;
    [SerializeField] private bool multiSpawner;
    [SerializeField] private bool independentSpawns;

    private List<GameObject> _spawnInstances = new();

    bool ITriggerable.IsOn => _spawnInstances.Count > 0;

    void ITriggerable.TriggerOn()
    {
        if (!multiSpawner && _spawnInstances.Count > 0)
        {
            Debug.Log($"{nameof(Spawner)} on {gameObject.name} can only have 1 child spawned at a time.");
        }
        else
        {
            _spawnInstances.Add(Instantiate(spawnPrefab, parent));
        }
    }

    void ITriggerable.TriggerOff()
    {
        if (_spawnInstances.Count > 0)
        {
            _spawnInstances.ForEach(e => Destroy(e));
            _spawnInstances.Clear();
        }
        else
        {
            Debug.Log($"{nameof(Spawner)} on {gameObject.name} has no child to destroy.");
        }
    }

    private void OnDisable()
    {
        if (!independentSpawns)
        {
            _spawnInstances.ForEach(e => Destroy(e));
            _spawnInstances.Clear();
        }
    }
}
