using System.Collections.Generic;
using UnityEngine;

public class BehaviourOperation : Operation
{
    [SerializeField] private List<Behaviour> componentsToEnable = new();
    [SerializeField] private List<Behaviour> componentsToDisable = new();
    [SerializeField] private List<Behaviour> componentsToDestroy = new();

    public override void Execute()
    {
        componentsToEnable.ForEach(e => e.enabled = true);
        componentsToDisable.ForEach(e => e.enabled = false);
        componentsToDestroy.ForEach(e => Destroy(e));
    }
}
