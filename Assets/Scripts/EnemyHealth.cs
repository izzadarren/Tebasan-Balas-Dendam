using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyHealth : MonoBehaviour
{
    [Tooltip("HP musuh. Setelah 0 musuh akan 'hilang'")]
    public int maxHP = 3;

    private int currentHP;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        currentHP -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. HP={currentHP}/{maxHP}");

        if (currentHP <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        // opsi: play death VFX / disable AI first supaya tidak bergerak/serang lagi
        var ai = GetComponent<EnemyPatrol2D>();
        if (ai != null) ai.enabled = false;

        // disable colliders so it doesn't interact anymore
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        // optionally play animation/effect here, then destroy
        Destroy(gameObject);
    }
}
