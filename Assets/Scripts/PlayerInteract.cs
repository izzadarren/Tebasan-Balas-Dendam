using System.Reflection;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Tooltip("UI GameObject (Canvas or panel) yang akan ditampilkan")]
    [SerializeField] private GameObject interactUI;

    [Tooltip("Tombol untuk interaksi")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Tooltip("Tag yang dikenali sebagai pemain")]
    [SerializeField] private string playerTag = "Player";

    private bool playerNearby = false;
    private PlayerController playerController = null;

    void Start()
    {
        if (interactUI != null)
            interactUI.SetActive(false);
    }

    void Update()
    {
        // Jika UI sedang tampil, tekan E akan menutup walau player tidak lagi dekat
        if (Input.GetKeyDown(interactKey))
        {
            if (interactUI != null && interactUI.activeSelf)
            {
                ToggleUI(false);
                return;
            }

            // jika UI belum tampil, hanya boleh membuka bila player dekat
            if (playerNearby)
            {
                ToggleUI(true);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerNearby = true;
            // simpan referensi PlayerController (jangan ambil generic MonoBehaviour)
            playerController = other.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerNearby = false;
            // jangan reset playerController jika UI masih terbuka; biarkan ToggleUI memanggil UnlockMovement
            if (interactUI == null || !interactUI.activeSelf)
                playerController = null;
        }
    }

    // Tampilkan atau sembunyikan UI; dipanggil juga dari tombol Close di UI
    public void ToggleUI(bool show)
    {
        if (interactUI == null) return;

        interactUI.SetActive(show);

        // coba panggil LockMovement/UnlockMovement pada PlayerController jika tersedia
        if (playerController != null)
        {
            var mi = playerController.GetType().GetMethod(show ? "LockMovement" : "UnlockMovement", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi != null)
            {
                try { mi.Invoke(playerController, null); }
                catch { /* ignore invocation errors */ }
            }
        }
    }

    // helper supaya button di UI bisa menutup
    public void CloseUI()
    {
        ToggleUI(false);
    }

    private void OnDrawGizmosSelected()
    {
        // visual hint untuk object interaksi (jika memiliki collider trigger)
        Gizmos.color = Color.cyan;
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
