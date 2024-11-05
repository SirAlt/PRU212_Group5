using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(69420)]
public class VictoryScreen : MonoBehaviour
{
    [SerializeField] private string titleSceneName;
    [SerializeField] private float fadeInTime;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        StartCoroutine(nameof(FadeIn));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator FadeIn()
    {
        var interval = 0.05f;
        var timer = 0f;
        while (timer <= fadeInTime)
        {
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInTime);
            yield return new WaitForSecondsRealtime(interval);
            timer += interval;
        }
    }

    public void Show()
    {
        PauseManager.Instance.Pause();
        gameObject.SetActive(true);
    }

    public void ReturnToTitle()
    {
        PauseManager.Instance.Unpause();
        SceneManager.LoadScene(titleSceneName);
    }
}
