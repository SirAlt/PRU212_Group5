using TMPro;
using UnityEngine;

public class HealthText : MonoBehaviour
{
    [SerializeField] private float movedSpeed = 75f;
    [SerializeField] private float timeToFace = 1f;

    private RectTransform textTranform;
    private TextMeshProUGUI textMeshPro;
    private Color startColor;

    private float timeElapsed;

    private void Awake()
    {
        textTranform = GetComponent<RectTransform>();
        textMeshPro = GetComponent<TextMeshProUGUI>();
        startColor = textMeshPro.color;
    }

    void Update()
    {
        textTranform.position += movedSpeed * Time.deltaTime * Vector3.up;

        timeElapsed += Time.deltaTime;
        if (timeElapsed < timeToFace)
        {
            float fadeAlpha = startColor.a * (1 - (timeElapsed / timeToFace));
            textMeshPro.color = new Color(startColor.r, startColor.g, startColor.b, fadeAlpha);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
