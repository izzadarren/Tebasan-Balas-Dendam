using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol2D : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] waypoints;
    public float patrolSpeed = 2f;
    public float reachDistance = 0.2f;

    [Header("Detection Settings")]
    public Transform player;
    public float detectionRange = 5f;
    public float attackRange = 1.2f;
    public float chaseSpeed = 3f;
    public float attackCooldown = 1.5f;

    private int currentIndex = 0;
    private Rigidbody2D rb;
    private float lastAttackTime;
    private bool isPlayerDetected;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (player == null) 
        {
            Patrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(rb.position, player.position);
        isPlayerDetected = distanceToPlayer <= detectionRange;

        if (isPlayerDetected)
        {
            if (distanceToPlayer > attackRange)
                ChasePlayer();
            else
                AttackPlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Vector2 targetPos = waypoints[currentIndex].position;
        Vector2 direction = (targetPos - rb.position).normalized;

        rb.linearVelocity = direction * patrolSpeed;

        float distance = Vector2.Distance(rb.position, targetPos);
        if (distance <= reachDistance)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * chaseSpeed;
    }

    void AttackPlayer()
    {
        rb.linearVelocity = Vector2.zero; // berhenti saat menyerang

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            Debug.Log("Enemy menyerang player!");
            // TODO: Tambahkan animasi atau logika damage ke player di sini
        }
    }

    void OnDrawGizmos()
    {
        if (waypoints != null && waypoints.Length >= 2)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.DrawSphere(waypoints[i].position, 0.1f);
                Gizmos.DrawLine(waypoints[i].position, waypoints[(i + 1) % waypoints.Length].position);
            }
        }

        // Radius deteksi dan serangan
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
