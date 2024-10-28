using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class LightingChange : MonoBehaviour, ITriggerable
{
    [SerializeField] private LayerMask triggerLayers;
    [SerializeField] private List<Light2D> lightsToTurnOff;
    [SerializeField] private List<Light2D> lightsToTurnOn;

    [SerializeField] private float duration = 3.0f;
    [SerializeField] private float interval = 0.1f;

    private Collider2D _detector;

    private bool _isOn;

    bool ITriggerable.IsOn => _isOn;

    void ITriggerable.TriggerOn()
    {
        if (!_isOn) StartCoroutine(nameof(DarknessEngulfs));
    }

    void ITriggerable.TriggerOff()
    {
        if (_isOn) StartCoroutine(nameof(EndOfTheTunnel));
    }

    private void Awake()
    {
        _detector = GetComponent<Collider2D>();
    }

    private void Start()
    {
        _detector.isTrigger = true;
        _detector.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isOn) return;
        if ((triggerLayers & (1 << collision.gameObject.layer)) != 0)
        {
            StartCoroutine(nameof(DarknessEngulfs));
            _isOn = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_isOn) return;
        if ((triggerLayers & (1 << collision.gameObject.layer)) != 0)
        {
            StartCoroutine(nameof(EndOfTheTunnel));
            _isOn = false;
        }
    }

    private IEnumerator DarknessEngulfs()
    {
        var step = interval / duration;
        var timer = duration;
        while (timer > 0.0f)
        {
            lightsToTurnOff.ForEach(e => e.intensity -= step);
            lightsToTurnOn.ForEach(e => e.intensity += step);

            timer -= interval;
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator EndOfTheTunnel()
    {
        var step = interval / duration;
        var timer = duration;
        while (timer > 0.0f)
        {
            // Reverse the changes.
            lightsToTurnOff.ForEach(e => e.intensity += step);
            lightsToTurnOn.ForEach(e => e.intensity -= step);

            timer -= interval;
            yield return new WaitForSeconds(interval);
        }
    }
}
