using UnityEngine;

public class ScanAstarGraphOperation : Operation
{
    [SerializeField] private string graphName;

    public override void Execute()
    {
        var graph = AstarPath.active.data.FindGraph(e => e.name == graphName);
        AstarPath.active.Scan(graph);
    }
}
