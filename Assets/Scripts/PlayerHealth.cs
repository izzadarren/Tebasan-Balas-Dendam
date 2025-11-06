using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    public int health;
    public int maxHealth = 100;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage (int amount)
    {
        health -= amount;
        if (health < 0)
        {
            Destroy(gameObject); 
        }
    }
    // Update is called once per frame

}
