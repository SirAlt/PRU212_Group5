using UnityEngine;

public interface IMovement
{
    Collider2D Collider { set; }
    CollisionConfigs Configurations { get; }

    Vector2 EnvironmentVelocity { get; set; }

    void AddLayersX(LayerMask newLayers);
    void AddLayersY(LayerMask newLayers);
    void RemoveLayersX(LayerMask layersToRemove);
    void RemoveLayersY(LayerMask layersToRemove);

    void Move(Vector2 velocity);
}
