using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private DialogueUI dialogueUI;

    public DialogueUI DialogueUI => dialogueUI;
    public IInteractable Interactable { get; set; }

    public float moveSpeed = 5f;
    public float collisionOffset = 0.05f;
    public ContactFilter2D movementFilter;

    // FOOTSTEP: assign an AudioClip in Inspector; an AudioSource will be created automatically if none assigned
    [Header("Footstep Sound")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioSource footstepSource;
    [Tooltip("Seconds between footstep sounds while moving")]
    [SerializeField] private float stepInterval = 0.35f;

    [Header("Combat")]
    [Tooltip("Enable/disable player's ability to attack")]
    [SerializeField] private bool attackEnabled = true;
    [Tooltip("If true use an Animator bool parameter for attack; otherwise use trigger 'swordAttack'")]
    [SerializeField] private bool attackUsesBool = false;
    [SerializeField] private string attackBoolName = "isAttacking";
    [Tooltip("Fallback auto-end attack after this many seconds (0 = rely on animation event)")]
    [SerializeField] private float attackDuration = 0f;

    Vector2 movementInput;
    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    public bool CanMove = true;

    // internal timer for footsteps
    private float stepTimer = 0f;

    // flag hasil movement di FixedUpdate untuk dipakai di Update
    private bool movedThisFixed = false;

    // runtime
    private bool isAttacking = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ensure we have an AudioSource to play footsteps
        if (footstepSource == null)
        {
            if (footstepClip != null)
            {
                footstepSource = gameObject.AddComponent<AudioSource>();
                footstepSource.playOnAwake = false;
                footstepSource.clip = footstepClip;
                footstepSource.loop = false;
            }
        }
    }
    void Update()
    {
        // if dialogue is open, stop movement but allow "skip" input
        if (dialogueUI != null && dialogueUI.IsOpen)
        {
            movementInput = Vector2.zero;
            movedThisFixed = false;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                dialogueUI.Skip();
            }
            return;
        }

        // input / dialog handling (tetap di Update)
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"E pressed. Interactable={(Interactable != null ? Interactable.GetType().Name : "null")}, DialogueOpen={(dialogueUI != null ? dialogueUI.IsOpen.ToString() : "null")}");
            if (Interactable != null && (dialogueUI == null || !dialogueUI.IsOpen))
            {
                Interactable.Interact(this);
            }
        }

        // gunakan movedThisFixed hasil FixedUpdate untuk set animator dan footsteps secara responsif
        bool isMoving = CanMove && movedThisFixed;

        if (animator != null)
            animator.SetBool("IsMoving", isMoving);

        HandleFootstepsUpdate(isMoving, Time.deltaTime);

        // flip sprite responsive di Update (no physics)
        if (movementInput.x < 0)
            spriteRenderer.flipX = true;
        else if (movementInput.x > 0)
            spriteRenderer.flipX = false;
    }

    private void FixedUpdate()
    {
        movedThisFixed = false; // reset sebelum cek

        if (!CanMove || rb == null) return;

        if (movementInput != Vector2.zero)
        {
            bool success = TryMove(movementInput);
            if (!success && movementInput.x != 0)
            {
                success = TryMove(new Vector2(movementInput.x, 0));
            }
            if (!success && movementInput.y != 0)
            {
                success = TryMove(new Vector2(0, movementInput.y));
            }

            // simpan hasil agar Update bisa membaca segera
            movedThisFixed = success;
        }
        else
        {
            movedThisFixed = false;
        }
    }

    // ganti HandleFootsteps supaya menerima deltaTime dari Update dan bisa Stop() saat berhenti
    private void HandleFootstepsUpdate(bool isMoving, float deltaTime)
    {
        if (footstepClip == null || footstepSource == null) return;

        if (isMoving)
        {
            stepTimer += deltaTime;
            if (stepTimer >= stepInterval)
            {
                // prevent overlapping playback
                if (!footstepSource.isPlaying)
                {
                    footstepSource.PlayOneShot(footstepClip);
                    // Debug.Log("Footstep played");
                }
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
            if (footstepSource.isPlaying)
                footstepSource.Stop();
        }
    }

    private bool TryMove(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {

            int count = rb.Cast(
                    direction,
                    movementFilter,
                    castCollisions,
                    moveSpeed * Time.fixedDeltaTime + collisionOffset);
            if (count == 0)
            {
                rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime
                );
                return true;
            }
            else
            {
                return false;
            }
        } 
        else
        {
            return false;
        }

    }


    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();

    }
    void OnAttack()
    {
        // blocked if attack disabled, already attacking, can't move, or dialogue is open
        if (!attackEnabled || isAttacking || !CanMove || (dialogueUI != null && dialogueUI.IsOpen) || animator == null)
            return;

        // start attack
        LockMovement();
        isAttacking = true;

        if (attackUsesBool)
            animator.SetBool(attackBoolName, true);
        else
            animator.SetTrigger("swordAttack");

        if (attackDuration > 0f)
            StartCoroutine(EndAttackAfter(attackDuration));
    }

    // call this from an Animation Event at the end of the attack clip
    public void EndAttack()
    {
        if (attackUsesBool && animator != null)
            animator.SetBool(attackBoolName, false);

        isAttacking = false;
        UnlockMovement();
    }

    private IEnumerator EndAttackAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        EndAttack();
    }

    // allow enabling/disabling attack at runtime
    public void SetAttackEnabled(bool enabled)
    {
        attackEnabled = enabled;
        if (!attackEnabled)
        {
            // ensure attack state cleared immediately if disabling
            isAttacking = false;
            if (attackUsesBool && animator != null)
                animator.SetBool(attackBoolName, false);
        }
    }

    public void LockMovement()
    {
        CanMove = false;
    }
       public void UnlockMovement()
    {
        CanMove = true;
    }
}
