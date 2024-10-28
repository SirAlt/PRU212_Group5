using UnityEngine;

public interface IMover
{
    Vector2 MoveVector { get; }
    bool CalculateOnly { set; }
}
