using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TextFade : MonoBehaviour
{
    public event Action OnFadeInComplete;
    public event Action OnHoldComplete;
    public event Action OnFadeOutComplete;

    [SerializeField] private float fadeInTime;
    [SerializeField] private float stayTime;
    [SerializeField] private float fadeOutTime;

    [SerializeField] private bool autoFadeIn;
    [SerializeField] private bool autoFadeOut;

    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (autoFadeOut)
        {
            OnFadeInComplete += Hold;
            OnHoldComplete += FadeOut;
        }
        if (autoFadeIn) StartCoroutine(nameof(PerformFadeIn));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (autoFadeOut)
        {
            OnFadeInComplete -= Hold;
            OnHoldComplete -= FadeOut;
        }
    }

    private IEnumerator PerformFadeIn()
    {
        var timer = 0f;
        while (timer < fadeInTime)
        {
            _text.alpha = Mathf.Lerp(0f, 1f, timer / fadeInTime);
            timer += Time.deltaTime;
            yield return null;
        }
        OnFadeInComplete?.Invoke();
    }

    private IEnumerator PerformHold()
    {
        yield return new WaitForSeconds(stayTime);
        OnHoldComplete?.Invoke();
    }

    private IEnumerator PerformFadeOut()
    {
        var timer = 0f;
        while (timer < fadeOutTime)
        {
            _text.alpha = Mathf.Lerp(1f, 0f, timer / fadeInTime);
            timer += Time.deltaTime;
            yield return null;
        }
        OnFadeOutComplete?.Invoke();
    }

    public void FadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(nameof(PerformFadeIn));
    }

    public void Hold()
    {
        StopAllCoroutines();
        StartCoroutine(nameof(PerformHold));
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(nameof(PerformFadeOut));
    }
}