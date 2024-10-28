using System.Collections.Generic;
using UnityEngine;

public class CheckpointSystem : MonoBehaviour
{
    #region Singleton

    public static CheckpointSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    #endregion

    private readonly List<Checkpoint> _checkpoints = new();

    public Checkpoint LastCheckpoint { get; private set; }

    private void Start()
    {
        _checkpoints.AddRange(GetComponentsInChildren<Checkpoint>());
        SetLastCheckpoint(_checkpoints[0]);
    }

    public void SetLastCheckpoint(Checkpoint checkpoint)
    {
        if (LastCheckpoint == checkpoint) return;
        LastCheckpoint = checkpoint;
        ActivateSingleCheckpoint();
    }

    private void ActivateSingleCheckpoint()
    {
        foreach (var cp in _checkpoints)
        {
            cp.Deactivate();
        }
        LastCheckpoint.Activate();
    }

    public void ResetCheckpoint() => SetLastCheckpoint(_checkpoints[0]);
}
