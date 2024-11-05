using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    private const float SilenceVolume = -80.0f;
    private const float MinVolume = -60.0f;
    private const float MaxVolume = 0.0f;

    private static readonly float MinSliderValue;

    static SettingsMenu()
    {
        // 1. [MinLoudness-MaxLoudness] ~ [0-1]
        // 2. Loudness = 2 ^ (dB/10)
        // => MaxLoudness = f(MaxVolume)
        //    1/2 * MaxLoudness = f(MaxVolume - 10)
        //    1/4 * MaxLoudness = f(MaxVolume - 20)
        //    .
        //    .
        //    .
        //    1/2^k * MaxLoudness = f(MaxVolume - 10k)
        // => k = (MaxVolume - CurVolume)/10
        //    Let MaxLoudness = NormalizedMaxLoudness = 1
        //    Then NormalizedCurLoudness = 1/2^((MaxVolume - CurVolume)/10) = 2^((CurVolume-MaxVolume)/10)
        //    
        // Given that the RHS > 0, but we want our slider to reach 0: We must snap the low end to 0.
        // Therefore, any slider value smaller than NormalizedMinLoudness must yield SilenceVolume.

        MinSliderValue = Mathf.Pow(2.0f, (MinVolume - MaxVolume) / 10.0f);
    }

    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private void Start()
    {
        var bgmSliderValue = SliderValueFromVolume(SettingsManager.Instance.BgmVolume);
        var sfxSliderValue = SliderValueFromVolume(SettingsManager.Instance.SfxVolume);
        bgmVolumeSlider.SetValueWithoutNotify(bgmSliderValue);
        sfxVolumeSlider.SetValueWithoutNotify(sfxSliderValue);
    }

    public void SetBgmVolume(float sliderValue) => SettingsManager.Instance.BgmVolume = VolumeFromSliderValue(sliderValue);
    public void SetSfxVolume(float sliderValue) => SettingsManager.Instance.SfxVolume = VolumeFromSliderValue(sliderValue);

    public void SaveSettings() => SettingsManager.Instance.SaveSettings();

    private static float SliderValueFromVolume(float volume) => Mathf.Pow(2, volume / 10.0f);

    private static float VolumeFromSliderValue(float sliderValue)
    {
        // +10 dB <-> x2 perceived loudness
        // Perceived loudness = 2 ^ (dB/10)
        // => dB = 10 * log2(loudness)
        //
        // See static ctor for more details
        float volume;
        if (sliderValue == 0) volume = SilenceVolume;
        else if (sliderValue <= MinSliderValue) volume = MinVolume;
        else volume = 10.0f * (float)Math.Log(sliderValue, 2.0f);
        return volume;
    }
}
