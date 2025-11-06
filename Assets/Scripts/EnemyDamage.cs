using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    // Variabel untuk menentukan jumlah damage yang diberikan ke player
    public int damageAmount;

    // Method ini akan dipanggil saat enemy bertabrakan dengan objek lain yang memiliki Collider2D (dengan Trigger aktif)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Periksa apakah objek yang bertabrakan memiliki tag "Player"
        if (collision.CompareTag("Player"))
        {
            // Mencari komponen PlayerHealth pada objek yang bertabrakan
            PlayerHealth player = collision.GetComponent<PlayerHealth>();

            // Jika objek memiliki komponen PlayerHealth, berikan damage ke player
            if (player != null)
            {
                player.TakeDamage(damageAmount); // Kurangi health player sesuai damageAmount
            }
        }
    }
}

