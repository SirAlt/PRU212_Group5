using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-666)]
public class SoundManager : MonoBehaviour
{
    #region Singleton

    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            OnAwake();
        }
        else
        {
            Destroy(this);
        }
    }

    #endregion

    [SerializeField] private AudioMixerGroup bgmMixer;
    [SerializeField] private AudioMixerGroup sfxMixer;

    [SerializeField] private Sound[] sounds;

    private AudioSource _bgmAudioSource;

    private Dictionary<string, Sound> _bgm = new();
    private Dictionary<string, Sound> _sfx = new();

    public string CurrentBgmTrack;

    private void OnAwake()
    {
        _bgmAudioSource = gameObject.AddComponent<AudioSource>();
        _bgmAudioSource.outputAudioMixerGroup = bgmMixer;
        _bgmAudioSource.loop = true;

        foreach (var sound in sounds)
        {
            if (sound.soundType == Sound.SoundType.BGM)
            {
                sound.source = _bgmAudioSource;
                _bgm[sound.name] = sound;
            }
            else if (sound.soundType == Sound.SoundType.SFX)
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = sfxMixer;

                audioSource.clip = sound.clip;
                audioSource.volume = sound.volume;
                audioSource.pitch = sound.pitch;
                audioSource.loop = sound.loop;

                sound.source = audioSource;
                _sfx[sound.name] = sound;
            }
        }
    }

    public void PlayBgm(string name)
    {
        _bgmAudioSource.clip = _bgm[name].clip;
        _bgmAudioSource.Play();
        CurrentBgmTrack = name;
    }

    public void StopBgm()
    {
        _bgmAudioSource.Stop();
        CurrentBgmTrack = null;
    }

    public void PlaySfx(string name)
    {
        _sfx[name].source.Play();
    }

    public void StopSfx(string name)
    {
        _sfx[name].source.Stop();
    }
}
