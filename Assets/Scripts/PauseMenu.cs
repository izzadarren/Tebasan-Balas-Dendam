using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject container;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            container.SetActive(true);
            Time.timeScale = 0; // Pause the game
        }
    }

    public void ResumeButton()
    {
        container.SetActive(false);
        Time.timeScale = 1; // Resume the game
    }

    public void MainMenuButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}

