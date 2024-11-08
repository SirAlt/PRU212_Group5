using UnityEngine;

public class DelayedStart : MonoBehaviour
{
    [SerializeField] private float _startupDelay;

    private void Start()
    {
        if (_startupDelay > 0)
        {
            gameObject.SetActive(false);
            Invoke(nameof(Startup), _startupDelay);
        }
    }

    private void Startup()
    {
        gameObject.SetActive(true);
    }
}
