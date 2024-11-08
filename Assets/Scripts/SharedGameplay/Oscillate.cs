using UnityEngine;

public class Oscillate : MonoBehaviour
{
    [SerializeField] private float amplitude;
    [SerializeField] private float speed;
    [SerializeField] private Vector3 direction;
    [SerializeField] private UpdateMode updateMode;

    private enum UpdateMode
    {
        Update,
        FixedUpdate,
        LateUpdate,
    }

    private float _timer;
    private float _curAmpFactor;
    private Vector3 _origLocalPos;

    private void Awake()
    {
        direction = direction.normalized;
        _origLocalPos = transform.localPosition;
    }

    private void OnDisable()
    {
        transform.localPosition = _origLocalPos;
    }

    private void Update()
    {
        if (updateMode == UpdateMode.Update)
        {
            _timer += Time.deltaTime;
            PerformOscillation();
        }
    }

    private void FixedUpdate()
    {
        if (updateMode == UpdateMode.FixedUpdate)
        {
            _timer += Time.fixedDeltaTime;
            PerformOscillation();
        }
    }

    private void LateUpdate()
    {
        if (updateMode == UpdateMode.LateUpdate)
        {
            _timer += Time.deltaTime;
            PerformOscillation();
        }
    }

    private void PerformOscillation()
    {
        _curAmpFactor = Mathf.Sin(_timer * speed);
        var offset = _curAmpFactor * amplitude * direction;
        transform.localPosition = _origLocalPos + offset;
    }
}
