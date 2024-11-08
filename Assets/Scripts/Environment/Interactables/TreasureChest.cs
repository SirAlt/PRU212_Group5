using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Collider2D))]
public class TreasureChest : MonoBehaviour
{
    [SerializeField] private ArtifactType type;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GameObject gemPrefab;

    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _animator.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((playerLayer & (1 << collision.gameObject.layer)) != 0
            && HasCorrectKey())
        {
            _animator.enabled = true;

            // TODO: Spawn gem. The gem will call this.
            MissionManager.Instance.OnGemObtained(type);

            Invoke(nameof(Disappear), 3f);
        }
    }

    private bool HasCorrectKey() => type switch
    {
        ArtifactType.Red => MissionManager.Instance.HasRedKey,
        ArtifactType.Blue => MissionManager.Instance.HasBlueKey,
        ArtifactType.Black => MissionManager.Instance.HasBlackKey,
        _ => false,
    };

    private void Disappear() => Destroy(gameObject);
}
