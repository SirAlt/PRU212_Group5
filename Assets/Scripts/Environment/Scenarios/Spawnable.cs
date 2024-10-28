using UnityEngine;

public class Spawnable : MonoBehaviour, ITriggerable
{
    bool ITriggerable.IsOn => gameObject.activeSelf;

    void ITriggerable.TriggerOn()
    {
        gameObject.SetActive(true);
    }

    void ITriggerable.TriggerOff()
    {
#if UNITY_EDITOR
        Debug.Log($"{nameof(Spawnable)} component on {gameObject.name} cannot be de-activated. Object is already \"spawned\".");
#endif
    }
}
