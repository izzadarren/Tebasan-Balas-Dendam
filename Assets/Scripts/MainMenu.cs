using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// Load scene Play (4 scene index forward)
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 4);
        Debug.Log("▶️ Loading Play scene...");
    }

    /// <summary>
    /// Load scene Credit
    /// </summary>
    public void CreditScene()
    {
        SceneManager.LoadScene("CreditScene");
        Debug.Log("📜 Loading Credit scene...");
    }

    /// <summary>
    /// Quit application
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("❌ Quitting game...");
        Application.Quit();
    }
}
