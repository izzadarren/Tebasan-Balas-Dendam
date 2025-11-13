using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Settings")]
    public int health = 1;
    public float hitFlashDuration = 0.1f;
    public Color hitColor = Color.red;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isHit = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int damage)
    {
        if (isHit) return; // mencegah double hit dalam satu waktu

        health -= damage;
        StartCoroutine(FlashRed());

        if (health <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashRed()
    {
        isHit = true;
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
        isHit = false;
    }

    private void Die()
    {
        // Bisa diganti efek animasi, drop item, dsb
        gameObject.SetActive(false);
        Debug.Log($"{gameObject.name} has been defeated!");
    }
}
