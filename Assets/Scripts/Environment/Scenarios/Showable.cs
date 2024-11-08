using UnityEngine;

public class Showable : MonoBehaviour, ITriggerable
{
    [SerializeField] private bool startActive;

    private void Start()
    {
        gameObject.SetActive(startActive);
    }

    bool ITriggerable.IsOn => gameObject.activeSelf;

    void ITriggerable.TriggerOn()
    {
        gameObject.SetActive(true);
    }

    void ITriggerable.TriggerOff()
    {
        gameObject.SetActive(false);
    }
}
