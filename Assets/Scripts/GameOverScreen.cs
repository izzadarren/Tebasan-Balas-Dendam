using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [Header("UI Panel Game Over")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Player Reference")]
    [SerializeField] private GameObject playerPrefab; // Prefab player untuk respawn
    private GameObject currentPlayer; // Reference ke player yang ada di scene
    private Vector3 deathPosition; // Posisi tempat player mati
    private Vector3 spawnPosition; // Posisi awal spawn player di scene

    private bool isGameOver = false;

    private void Start()
    {
        // Cari player di scene
        if (currentPlayer == null)
        {
            currentPlayer = GameObject.FindGameObjectWithTag("Player");
            if (currentPlayer == null)
            {
                Debug.LogError("❌ Player dengan tag 'Player' tidak ditemukan di scene!");
            }
            else
            {
                // Simpan posisi awal player saat game dimulai
                spawnPosition = currentPlayer.transform.position;
                Debug.Log($"📍 Spawn position disimpan: {spawnPosition}");
            }
        }

        // Sembunyikan panel game over saat start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    /// <summary>
    /// Panggil method ini saat player mati
    /// </summary>
    public void ShowGameOver(Vector3 playerDeathPosition)
    {
        if (isGameOver) return;

        isGameOver = true;
        deathPosition = playerDeathPosition;

        Debug.Log($"💀 Game Over! Player mati di posisi: {deathPosition}");

        // Pause game
        Time.timeScale = 0f;

        // Tampilkan panel game over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("❌ Game Over Panel tidak di-assign!");
        }

        // Tampilkan cursor
        Cursor.visible = true;
    }

    /// <summary>
    /// Restart game di posisi spawn awal
    /// </summary>
    public void RestartAtDeathPoint()
    {
        Debug.Log("🔄 Restarting scene...");

        // Resume game time in case it was paused
        Time.timeScale = 1f;

        // reload current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Kembali ke main menu
    /// </summary>
    public void GoToMainMenu()
    {
        Debug.Log("🏠 Going to Main Menu...");

        // Resume game
        Time.timeScale = 1f;

        // Load main menu scene
        SceneManager.LoadScene("MainMenu1");
    }
}