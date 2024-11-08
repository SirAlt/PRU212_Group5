using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class DarkZone : MonoBehaviour, ITriggerable
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
        _detector.enabled = true;
    }

    void ITriggerable.TriggerOff()
    {
        _detector.enabled = false;
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
        var timer = 0f;
        while (timer <= duration)
        {
            lightsToTurnOff.ForEach(e => e.intensity = Mathf.Lerp(e.intensity, 0f, timer / duration));
            lightsToTurnOn.ForEach(e => e.intensity = Mathf.Lerp(e.intensity, 1f, timer / duration));

            timer += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator EndOfTheTunnel()
    {
        var timer = 0f;
        while (timer <= duration)
        {
            // Reverse the changes.
            lightsToTurnOff.ForEach(e => e.intensity = Mathf.Lerp(e.intensity, 1f, timer / duration));
            lightsToTurnOn.ForEach(e => e.intensity = Mathf.Lerp(e.intensity, 0f, timer / duration));

            timer += interval;
            yield return new WaitForSeconds(interval);
        }
    }
}
