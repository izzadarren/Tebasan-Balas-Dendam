using UnityEngine;

public class PlayerAttackHitboxes : MonoBehaviour
{
    [SerializeField] private GameObject attackHitboxObject; // drag DownHitBox ke sini
    [SerializeField] private float attackDuration = 0.3f;

    private bool isAttacking = false;

    private void Start()
    {
        if (attackHitboxObject == null)
        {
            Debug.LogError("❌ Attack Hitbox belum di-assign di Inspector!");
            return;
        }

        attackHitboxObject.SetActive(false);
    }

    private void Update()
    {
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

        attackHitboxObject.SetActive(true);
        Debug.Log("✅ Hitbox diaktifkan");

        yield return new WaitForSeconds(attackDuration);

        attackHitboxObject.SetActive(false);
        Debug.Log("🛑 Hitbox dinonaktifkan");

        isAttacking = false;
    }
}
