using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    #region Singleton

    public static HitStop Instance { get; private set; }

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

    private bool _waiting;

    public void Stop(float duration)
    {
        if (_waiting) return;
        Time.timeScale = 0.0f;
        StartCoroutine(Wait(duration));
    }

    private IEnumerator Wait(float duration)
    {
        _waiting = true;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
        _waiting = false;
    }
}
