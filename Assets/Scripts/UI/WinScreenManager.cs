using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreenManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject winScreen; 
    
    public Button restartButton;
    public Button homeButton;

    private void Start()
    {
        winScreen.SetActive(false);
        restartButton.onClick.AddListener(PlayAgain);
        homeButton.onClick.AddListener(GoToMenu);
    }

    public void ShowWinScreen()
    {
        if(SceneManager.GetActiveScene().name == "Level4")
        {
            winScreen.SetActive(true); 
            Time.timeScale = 0; 
        }
        
    }

    public void PlayAgain()
    {
        Time.timeScale = 1; 
        SceneManager.LoadScene("Level1"); 
    }

    public void GoToMenu()
    {
        Time.timeScale = 1; 
        SceneManager.LoadScene("Menu"); 
    }

}
