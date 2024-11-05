using UnityEngine;

public class PauseManager : MonoBehaviour
{
    #region Singleton

    private PauseManager() { }

    public static PauseManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    #endregion

    public void Pause()
    {
        Time.timeScale = 0f;
    }

    public void Unpause()
    {
        Time.timeScale = 1f;
    }
}
