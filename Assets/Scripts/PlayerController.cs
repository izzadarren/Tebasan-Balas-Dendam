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
    [Tooltip("When attackDuration == 0 use this fallback to guarantee EndAttack is called")]
    [SerializeField] private float attackFallbackSeconds = 0.5f;

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
    private Coroutine attackMonitorCoroutine; // added

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ensure we have an AudioSource to play footsteps
        if (footstepSource == null && footstepClip != null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.playOnAwake = false;
            footstepSource.clip = footstepClip;
            footstepSource.loop = false;
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

        // interact / dialog input
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Interactable != null && (dialogueUI == null || !dialogueUI.IsOpen))
            {
                Interactable.Interact(this);
            }
        }

        bool isMoving = CanMove && movedThisFixed;

        if (animator != null)
            animator.SetBool("IsMoving", isMoving);

        HandleFootstepsUpdate(isMoving, Time.deltaTime);

        // flip sprite responsive in Update
        if (movementInput.x < 0)
            spriteRenderer.flipX = true;
        else if (movementInput.x > 0)
            spriteRenderer.flipX = false;
    }

    private void FixedUpdate()
    {
        movedThisFixed = false;

        if (!CanMove || rb == null) return;

        if (movementInput != Vector2.zero)
        {
            bool success = TryMove(movementInput);
            if (!success && movementInput.x != 0)
                success = TryMove(new Vector2(movementInput.x, 0));
            if (!success && movementInput.y != 0)
                success = TryMove(new Vector2(0, movementInput.y));

            movedThisFixed = success;
        }
        else
        {
            movedThisFixed = false;
        }
    }

    private void HandleFootstepsUpdate(bool isMoving, float deltaTime)
    {
        if (footstepClip == null || footstepSource == null) return;

        if (isMoving)
        {
            stepTimer += deltaTime;
            if (stepTimer >= stepInterval)
            {
                if (!footstepSource.isPlaying)
                    footstepSource.PlayOneShot(footstepClip);
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
        if (direction == Vector2.zero) return false;

        int count = rb.Cast(
            direction,
            movementFilter,
            castCollisions,
            moveSpeed * Time.fixedDeltaTime + collisionOffset);

        if (count == 0)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            return true;
        }
        return false;
    }

    void OnMove(InputValue movementValue)
    {
        // ensure this matches your InputAction name and PlayerInput behavior
        movementInput = movementValue.Get<Vector2>();
    }

    void OnAttack()
    {
        if (!attackEnabled || isAttacking || !CanMove || (dialogueUI != null && dialogueUI.IsOpen) || animator == null)
            return;

        LockMovement();
        isAttacking = true;

        if (attackUsesBool)
            animator.SetBool(attackBoolName, true);
        else
            animator.SetTrigger("swordAttack");

        // guarantee EndAttack will be called: use attackDuration if set, otherwise fallback
        float wait = (attackDuration > 0f) ? attackDuration : attackFallbackSeconds;

        if (attackMonitorCoroutine != null) StopCoroutine(attackMonitorCoroutine);
        attackMonitorCoroutine = StartCoroutine(EndAttackAfter(wait));
    }

    // call this from an Animation Event at the end of the attack clip (recommended)
    public void EndAttack()
    {
        if (attackMonitorCoroutine != null)
        {
            StopCoroutine(attackMonitorCoroutine);
            attackMonitorCoroutine = null;
        }

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

    private IEnumerator WaitForAttackAnimationEnd(float timeout)
    {
        float t = 0f;
        yield return null;
        while (t < timeout)
        {
            if (!IsPlayingAttackClip())
                break;
            t += Time.deltaTime;
            yield return null;
        }
        EndAttack();
    }

    private bool IsPlayingAttackClip()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;
        var clips = animator.GetCurrentAnimatorClipInfo(0);
        if (clips == null || clips.Length == 0) return false;
        var name = clips[0].clip.name.ToLower();
        return name.Contains("attack") || name.Contains("sword");
    }

    // allow enabling/disabling attack at runtime
    public void SetAttackEnabled(bool enabled)
    {
        attackEnabled = enabled;
        if (!attackEnabled)
        {
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

    private void OnDisable()
    {
        // safety: ensure player not left permanently locked
        EndAttack();
    }
}
