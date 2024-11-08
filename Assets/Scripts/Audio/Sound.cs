using System;
using UnityEngine;

[Serializable]
public class Sound
{
    public string name;
    public SoundType soundType;

    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(.1f, 3f)] public float pitch = 1f;
    public bool loop;

    public AudioSource source;

    public enum SoundType
    {
        BGM,
        SFX,
    }
}
