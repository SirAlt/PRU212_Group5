using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Collider2D))]
public class Key : MonoBehaviour
{
    [SerializeField] private ArtifactType type;
    [SerializeField] private LayerMask playerLayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((playerLayer & (1 << collision.gameObject.layer)) != 0)
        {
            MissionManager.Instance.OnKeyObtained(type);
            Destroy(gameObject);
        }
    }
}
