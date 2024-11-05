using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(69420)]
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private string titleSceneName;

    private void Start()
    {
        gameObject.SetActive(false);
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

    public void Resume()
    {
        PauseManager.Instance.Unpause();
        gameObject.SetActive(false);
    }

    public void RestartScene()
    {
        PauseManager.Instance.Unpause();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
