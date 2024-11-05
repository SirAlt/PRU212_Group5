using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathScreenManager : MonoBehaviour
{
    public GameObject deathScreen;
    public Button resumeButton;
    public Button restartButton;
    public Button homeButton;

    void Start()
    {
        // Ẩn màn hình chết khi bắt đầu
        deathScreen.SetActive(false);

        // Đăng ký sự kiện cho các nút
        resumeButton.onClick.AddListener(ResumeGame);
        restartButton.onClick.AddListener(RestartLevel);
        homeButton.onClick.AddListener(GoToHome);
    }

    public void ShowDeathScreen()
    {
        // Hiện màn hình chết
        deathScreen.SetActive(true);
        Time.timeScale = 0f; // Dừng thời gian game
    }

    public void ResumeGame()
    {
        // Tiếp tục game
        deathScreen.SetActive(false);
        Time.timeScale = 1f; // Tiếp tục thời gian game
    }

    public void RestartLevel()
    {
        // Tải lại scene hiện tại
        Time.timeScale = 1f; // Đảm bảo thời gian game chạy
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToHome()
    {
        // Tải scene Menu
        Time.timeScale = 1f; // Đảm bảo thời gian game chạy
        SceneManager.LoadScene("Menu"); // Đổi tên với scene menu của bạn
    }
}
