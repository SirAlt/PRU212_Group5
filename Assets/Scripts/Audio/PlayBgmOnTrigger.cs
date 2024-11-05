using UnityEngine;

public class PlayBgmOnTrigger : MonoBehaviour, ITriggerable
{
    [SerializeField] private string trackName;

    bool ITriggerable.IsOn => SoundManager.Instance.CurrentBgmTrack == trackName;

    void ITriggerable.TriggerOn()
    {
        if (!((ITriggerable)this).IsOn) SoundManager.Instance.PlayBgm(trackName);
    }

    void ITriggerable.TriggerOff()
    {
        if (((ITriggerable)this).IsOn) SoundManager.Instance.StopBgm();
    }
}
