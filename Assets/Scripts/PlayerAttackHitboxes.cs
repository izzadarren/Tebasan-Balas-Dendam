using UnityEngine;
using System.Collections;

public class PlayerAttackHitboxes : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDuration = 0.3f;
    [Tooltip("Delay sebelum hitbox aktif (wind-up)")]
    [SerializeField] private float attackWindup = 0.05f;
    [Tooltip("Delay setelah hitbox mati sebelum bisa menyerang lagi (cooldown)")]
    [SerializeField] private float attackCooldown = 0.15f;

    [Header("Hitboxes")]
    [SerializeField] private GameObject hitboxUp;
    [SerializeField] private GameObject hitboxDown;
    [SerializeField] private GameObject hitboxLeft;
    [SerializeField] private GameObject hitboxRight;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip attackSFX;
    private AudioSource attackSource;

    // added animator reference
    private Animator animator;

    // sprite renderer untuk flip X
    private SpriteRenderer spriteRenderer;

    private bool isAttacking = false;
    private Vector2 lastDirection = Vector2.down; // default arah

    // track posisi untuk menentukan arah berdasarkan perpindahan terakhir
    private Vector2 prevPosition;
    [Tooltip("Minimum sqr magnitude perpindahan untuk dianggap sebagai arah (default 0.0001)")]
    [SerializeField] private float minMoveSq = 0.0001f;

    private void Awake()
    {
        // memastikan prevPosition ter-set secepat mungkin
        prevPosition = transform.position;
    }

    public void SetAttackDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.01f)
            lastDirection = dir.normalized;

        // update flip segera saat arah di-set
        UpdateSpriteFlip();
    }

    private void Start()
    {
        DisableAllHitboxes();

        if (attackSFX != null)
        {
            attackSource = gameObject.AddComponent<AudioSource>();
            attackSource.playOnAwake = false;
            attackSource.clip = attackSFX;
            attackSource.volume = 1f;
        }

        // get Animator if ada
        animator = GetComponent<Animator>();

        // get SpriteRenderer jika ada (digunakan untuk flip horizontal)
        spriteRenderer = GetComponent<SpriteRenderer>();

        // init prevPosition di Start jika belum di-Awake
        prevPosition = transform.position;
    }

    private void Update()
    {
        // update lastDirection berdasarkan perpindahan posisi sejak frame sebelumnya
        Vector2 currentPos = (Vector2)transform.position;
        Vector2 delta = currentPos - prevPosition;
        if (delta.sqrMagnitude > minMoveSq)
        {
            lastDirection = delta.normalized;
            // tidak panggil UpdateSpriteFlip() di setiap frame berat, tapi lakukan untuk sinkron
            UpdateSpriteFlip();
        }
        prevPosition = currentPos;

        // hanya aktif saat klik kiri
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(HandleAttack());
        }
    }

    public void PlayAttackSound()
    {
        if (attackSource != null && attackSFX != null)
            attackSource.PlayOneShot(attackSFX);
    }

    private IEnumerator HandleAttack()
    {
        isAttacking = true;

        // trigger animasi segera agar tidak delay
        if (animator != null)
            animator.SetTrigger("Attack"); // pastikan ada Trigger "Attack" di Animator

        // Wind-up sebelum hitbox aktif (bisa 0)
        if (attackWindup > 0f)
            yield return new WaitForSeconds(attackWindup);

        // Jika tidak menggunakan Animation Events, fallback aktivasi hitbox di sini
        ActivateHitboxByDirection();
        PlayAttackSound();

        // Durasi hitbox aktif
        if (attackDuration > 0f)
            yield return new WaitForSeconds(attackDuration);

        DisableAllHitboxes();

        // Cooldown setelah serangan sebelum bisa menyerang lagi
        if (attackCooldown > 0f)
            yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    // public API untuk dipanggil dari Animation Event (lebih akurat)
    public void StartHitboxEvent()
    {
        ActivateHitboxByDirection();
        PlayAttackSound();
    }

    public void EndHitboxEvent()
    {
        DisableAllHitboxes();
    }

    private void ActivateHitboxByDirection()
    {
        DisableAllHitboxes();

        // pastikan flip sinkron saat menyerang
        UpdateSpriteFlip();

        if (Mathf.Abs(lastDirection.x) > Mathf.Abs(lastDirection.y))
        {
            if (lastDirection.x > 0 && hitboxRight != null)
                hitboxRight.SetActive(true);
            else if (hitboxLeft != null)
                hitboxLeft.SetActive(true);
        }
        else
        {
            if (lastDirection.y > 0 && hitboxUp != null)
                hitboxUp.SetActive(true);
            else if (hitboxDown != null)
                hitboxDown.SetActive(true);
        }
    }

    private void DisableAllHitboxes()
    {
        if (hitboxUp != null) hitboxUp.SetActive(false);
        if (hitboxDown != null) hitboxDown.SetActive(false);
        if (hitboxLeft != null) hitboxLeft.SetActive(false);
        if (hitboxRight != null) hitboxRight.SetActive(false);
    }

    // set flip X berdasarkan lastDirection.x
    private void UpdateSpriteFlip()
    {
        if (spriteRenderer != null)
        {
            // flip when moving/attacking left
            spriteRenderer.flipX = lastDirection.x < 0f;
        }
        else
        {
            // fallback: ubah localScale.x
            Vector3 ls = transform.localScale;
            ls.x = Mathf.Abs(ls.x) * (lastDirection.x < 0f ? -1f : 1f);
            transform.localScale = ls;
        }
    }
}
