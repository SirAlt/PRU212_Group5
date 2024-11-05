using UnityEngine;

public class PlayBgmOnEnable : MonoBehaviour
{
    [SerializeField] private string trackName;

    private void OnEnable()
    {
        SoundManager.Instance.PlayBgm(trackName);
    }
}
