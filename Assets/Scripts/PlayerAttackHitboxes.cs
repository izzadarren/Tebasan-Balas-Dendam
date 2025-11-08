using UnityEngine;

public class PlayerAttackHitboxes : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private GameObject attackHitboxObject; // drag DownHitBox ke sini
    [SerializeField] private float attackDuration = 0.3f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip attackSFX;
    private AudioSource attackSource;

    private bool isAttacking = false;

    private void Start()
    {
        // Check dan setup attack hitbox
        if (attackHitboxObject == null)
        {
            Debug.LogError("❌ Attack Hitbox belum di-assign di Inspector!");
            return;
        }
        attackHitboxObject.SetActive(false);

        // Setup audio source khusus untuk attack
        if (attackSFX != null)
        {
            Debug.Log("🎵 Setting up attack audio source...");
            attackSource = gameObject.AddComponent<AudioSource>();
            attackSource.playOnAwake = false;
            attackSource.clip = attackSFX;
            attackSource.volume = 1f; // Sesuaikan volume jika perlu
        }
        else
        {
            Debug.LogWarning("⚠️ Attack SFX belum di-assign di Inspector!");
        }
    }

    private void Update()
    {
        Debug.Log("🌀 Update berjalan");

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            // Pastikan objek valid
            if (attackHitboxObject == null)
            {
                Debug.LogError("❌ Attack Hitbox belum di-assign di Inspector!");
                return;
            }

            // Jangan aktifkan jika sudah aktif
            if (attackHitboxObject.activeSelf)
            {
                Debug.LogWarning("⚠️ Hitbox sudah aktif, abaikan serangan baru.");
                return;
            }

            StartCoroutine(ActivateHitbox());
        }
    }

    private System.Collections.IEnumerator ActivateHitbox()
    {
        isAttacking = true;

        // Play attack sound effect
        if (attackSource != null && attackSFX != null)
        {
            // Debug.Log("🔊 Playing attack sound...");
            attackSource.PlayOneShot(attackSFX);
        }
        else
        {
            Debug.LogWarning("⚠️ Can't play attack sound - missing source or clip");
        }

        attackHitboxObject.SetActive(true);
        // Debug.Log("✅ Hitbox diaktifkan");

        yield return new WaitForSeconds(attackDuration);

        attackHitboxObject.SetActive(false);
        // Debug.Log("🛑 Hitbox dinonaktifkan");

        isAttacking = false;
    }
}
