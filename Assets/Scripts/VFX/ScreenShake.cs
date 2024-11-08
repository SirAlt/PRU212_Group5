using Cinemachine;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    #region Singleton

    public static ScreenShake Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            OnAwake();
        }
        else
        {
            Destroy(this);
        }
    }

    #endregion

    private CinemachineVirtualCamera _camera;
    private float _startingIntensity;
    private float _shakeDuration;
    private float _shakeTimer;

    private void OnAwake()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
    }

    public void Shake(float intensity, float duration)
    {
        var cinemachinePerlinNoise = _camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachinePerlinNoise.m_AmplitudeGain = intensity;
        _startingIntensity = intensity;
        _shakeDuration = _shakeTimer = duration;
    }

    private void Update()
    {
        if (_shakeTimer > 0)
        {
            _shakeTimer -= Time.deltaTime;
            var cinemachinePerlinNoise = _camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            cinemachinePerlinNoise.m_AmplitudeGain = Mathf.Lerp(0f, _startingIntensity, _shakeTimer / _shakeDuration);
        }
    }
}
