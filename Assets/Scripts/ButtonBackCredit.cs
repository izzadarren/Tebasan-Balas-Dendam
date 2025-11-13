using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Script untuk button Back pada UI.
/// Menangani navigasi kembali ke scene sebelumnya atau menutup UI.
/// </summary>
public class ButtonBackCredit : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject uiPanelToClose; // Panel yang akan di-close
    [SerializeField] private bool loadPreviousScene = false; // Jika true, load scene sebelumnya
    [SerializeField] private string sceneNameToLoad; // Nama scene untuk di-load (alternatif)

    private void Start()
    {
        // Auto-detect button component jika belum di-assign
        if (backButton == null)
        {
            backButton = GetComponent<Button>();
        }

        // Register button click listener
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            Debug.Log("✅ Back button siap digunakan");
        }
        else
        {
            Debug.LogError("❌ Button component tidak ditemukan pada GameObject ini!");
        }
    }

    /// <summary>
    /// Method yang dipanggil saat button Back diklik
    /// </summary>
    private void OnBackButtonClicked()
    {
        Debug.Log("🔙 Back button diklik");

        // Opsi 1: Close UI Panel jika ada
        if (uiPanelToClose != null)
        {
            uiPanelToClose.SetActive(false);
            Debug.Log("🔙 UI Panel ditutup");
            return;
        }

        // Opsi 2: Load scene berdasarkan nama
        if (!string.IsNullOrEmpty(sceneNameToLoad))
        {
            SceneManager.LoadScene(sceneNameToLoad);
            Debug.Log($"🔙 Loading scene: {sceneNameToLoad}");
            return;
        }

        // Opsi 3: Load scene sebelumnya
        if (loadPreviousScene)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (currentSceneIndex > 0)
            {
                SceneManager.LoadScene(currentSceneIndex - 1);
                Debug.Log($"🔙 Loading previous scene (index: {currentSceneIndex - 1})");
            }
            else
            {
                Debug.LogWarning("⚠️ Tidak ada scene sebelumnya!");
            }
            return;
        }

        Debug.LogWarning("⚠️ Tidak ada aksi back button yang dikonfigurasi!");
    }

    /// <summary>
    /// Method public untuk menutup UI dari script lain
    /// </summary>
    public void BackToMenu()
    {
        OnBackButtonClicked();
    }

    /// <summary>
    /// Quit application
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("❌ Quitting game...");
        Application.Quit();
    }

    private void OnDestroy()
    {
        // Hapus listener saat destroy untuk menghindari memory leak
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackButtonClicked);
        }
    }
}