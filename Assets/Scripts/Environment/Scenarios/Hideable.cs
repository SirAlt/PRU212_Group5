using UnityEngine;

public class Hideable : MonoBehaviour, ITriggerable
{
    [SerializeField] private bool startHidden;

    private void Start()
    {
        gameObject.SetActive(!startHidden);
    }

    bool ITriggerable.IsOn => !gameObject.activeSelf;

    void ITriggerable.TriggerOn()
    {
        gameObject.SetActive(false);
    }

    void ITriggerable.TriggerOff()
    {
        gameObject.SetActive(true);
    }
}
