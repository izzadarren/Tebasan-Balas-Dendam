using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth = 100;

    [Header("UI References")]
    public Slider healthSlider; // Referensi ke UI Slider untuk Health Bar
    public Image healthBarFill; // Opsional, kalau kamu mau tetap pakai Image Fill

    private void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"[PlayerHealth] Took {amount} damage. HP={currentHealth}/{maxHealth}");
        UpdateHealthUI();

        if (currentHealth <= 0)
            Die();
    }

    // Compatibility method (some enemies call ApplyDamage)
    public void ApplyDamage(int amount)
    {
        TakeDamage(amount);
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (healthBarFill != null)
        {
            float t = (maxHealth > 0) ? (float)currentHealth / maxHealth : 0f;
            healthBarFill.fillAmount = Mathf.Clamp01(t);
        }
    }

    private void Die()
    {
        Debug.Log("[PlayerHealth] Player died.");
        // simple default behaviour: disable GameObject (customize as needed)
        gameObject.SetActive(false);
    }
}
