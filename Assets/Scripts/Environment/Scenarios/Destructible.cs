using UnityEngine;

public class Destructible : MonoBehaviour, ITriggerable
{
    [SerializeField] private bool actuallyDestroy;

    bool ITriggerable.IsOn => !gameObject.activeSelf;

    void ITriggerable.TriggerOn()
    {
        if (actuallyDestroy)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    void ITriggerable.TriggerOff()
    {
#if UNITY_EDITOR
        Debug.Log($"{nameof(Destructible)} component on {gameObject.name} cannot be re-activated. Object is already \"destroyed\".");
#endif
    }
}
