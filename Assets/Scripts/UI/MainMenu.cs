using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string startSceneName = "Level1";

    private void Start()
    {
        // DonÅft call AudioMixer.SetFloat in the following event functions as it can result in unexpected behavior:
        //     MonoBehaviour.Awake
        //     OnEnable
        //     RuntimeInitializeLoadType.AfterSceneLoad
        // cf. https://docs.unity3d.com/ScriptReference/Audio.AudioMixer.SetFloat.html
        SettingsManager.Instance.LoadSettings();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(startSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}
