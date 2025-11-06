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
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    // Method untuk menerima damage
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Method untuk memperbarui tampilan Health Bar
    void UpdateHealthUI()
    {
        float fillAmount = (float)currentHealth / maxHealth;

        // Kalau pakai Slider
        if (healthSlider != null)
        {
            healthSlider.value = fillAmount; // value slider dari 0 - 1
        }

        // Kalau masih mau pakai Image Fill (opsional)
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = fillAmount; // gunakan fillAmount, bukan scale
        }
    }

    // Method ketika Player mati
    void Die()
    {
        Debug.Log("Player Mati!");
        gameObject.SetActive(false);
    }
}
