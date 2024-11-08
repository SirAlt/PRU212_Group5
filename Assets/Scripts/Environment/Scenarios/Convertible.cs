using UnityEngine;

public class Convertible : MonoBehaviour, ITriggerable
{
    [SerializeField] private bool startActive;

    private void Start()
    {
        gameObject.SetActive(startActive);
    }

    bool ITriggerable.IsOn => gameObject.activeSelf;

    void ITriggerable.TriggerOn() => Switch();

    void ITriggerable.TriggerOff() => Switch();

    private void Switch()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
