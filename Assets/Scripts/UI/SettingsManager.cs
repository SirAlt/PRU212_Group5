using System;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    #region Singleton 

    private SettingsManager() { }

    public static SettingsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    public event Action OnSettingsInitialized;

    [SerializeField] private AudioMixer _masterMixer;

    public bool Initialized { get; private set; }

    private float _bgmVolume;
    public float BgmVolume
    {
        get => _bgmVolume;
        set
        {
            _bgmVolume = value;
            if (_masterMixer != null)
            {
                _masterMixer.SetFloat("BgmVolume", value);
            }
        }
    }

    private float _sfxVolume;
    public float SfxVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = value;
            if (_masterMixer != null)
            {
                _masterMixer.SetFloat("SfxVolume", value);
            }
        }
    }

    public void LoadSettings()
    {
        BgmVolume = PlayerPrefs.GetFloat("BgmVolume");
        SfxVolume = PlayerPrefs.GetFloat("SfxVolume");
        if (!Initialized)
        {
            Initialized = true;
            OnSettingsInitialized?.Invoke();
        }
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("BgmVolume", BgmVolume);
        PlayerPrefs.SetFloat("SfxVolume", SfxVolume);
    }
}
