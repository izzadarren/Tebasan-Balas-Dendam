using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Hit something: " + other.name);

     if (other.gameObject.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
            enemyHealth.TakeDamage(damageAmount);
        }
    }public class AttackHitboxController : MonoBehaviour
{
    public GameObject hitbox;

    public void EnableHitbox() { hitbox.SetActive(true); }
    public void DisableHitbox() { hitbox.SetActive(false); }
}

}
