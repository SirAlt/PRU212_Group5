using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private Vector3 rotation;
    [SerializeField] private UpdateMode updateMode;

    private enum UpdateMode
    {
        Update,
        FixedUpdate,
        LateUpdate,
    }

    private Vector3 _origEulers;

    private void Awake()
    {
        _origEulers = transform.eulerAngles;
    }

    private void OnDisable()
    {
        transform.eulerAngles = _origEulers;
    }

    private void Update()
    {
        if (updateMode == UpdateMode.Update)
            transform.eulerAngles += rotation;
    }

    private void FixedUpdate()
    {
        if (updateMode == UpdateMode.FixedUpdate)
            transform.eulerAngles += rotation;
    }

    private void LateUpdate()
    {
        if (updateMode == UpdateMode.LateUpdate)
            transform.eulerAngles += rotation;
    }
}
