using UnityEngine;
using static Constants;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class EndPortal : MonoBehaviour
{
    private SpriteRenderer _sprite;
    private Collider2D _detector;

    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _detector = GetComponent<Collider2D>();
    }

    public void SetActive(bool active)
    {
        _sprite.enabled = active;
        _detector.enabled = active;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.parent.CompareTag(PlayerTag))
        {
            MissionManager.Instance.OnLevelCompleted();
        }
    }
}
