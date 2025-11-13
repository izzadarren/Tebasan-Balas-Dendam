using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private const string PARAM_MOVE_X = "MoveX";
    private const string PARAM_MOVE_Y = "MoveY";
    private const string PARAM_IS_MOVING = "isMoving";
    private const string PARAM_ATTACK = "Attack";
    private const string PARAM_ATTACK_DIR = "AttackDirection";

    [Header("Movement")]
    public float moveSpeed = 5f;
    public bool CanMove = true;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movementInput;
    private Vector2 lastMoveDirection = Vector2.down; // default menghadap bawah

    // external interaction point (restores API yang DialogueActivator dipanggil)
    public IInteractable Interactable { get; set; }

    [Header("Combat")]
    [SerializeField] private PlayerAttackHitboxes attackHitboxes;
    [SerializeField] private bool attackEnabled = true;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private DialogueUI dialogueUI;
    public DialogueUI DialogueUI => dialogueUI;

    [Header("Audio")]
    [Tooltip("Footstep clips - one will be chosen when starting to walk")]
    [SerializeField] private AudioClip[] footstepClips;
    [Tooltip("Optional: use a single clip for continuous loop (preferred)")]
    [SerializeField] private AudioClip footstepLoopClip;
    [SerializeField] private float footstepVolume = 0.6f;
    [SerializeField] private bool loopFootsteps = true; // loop while walking
    [Tooltip("Optional: assign a dedicated AudioSource for footsteps (recommended). If empty a child AudioSource will be created.")]
    [SerializeField] private AudioSource footstepSource;

    private bool isAttacking = false;
    private Coroutine attackCoroutine;

    // --- new fields for immediate walk switching ---
    private int prevMoveDir = -1;
    private static readonly int HASH_WALK_DOWN = Animator.StringToHash("player_walkdown");
    private static readonly int HASH_WALK_UP = Animator.StringToHash("player_walkup");
    private static readonly int HASH_WALK_RIGHT = Animator.StringToHash("player_walkright");
    private static readonly int HASH_WALK_LEFT = Animator.StringToHash("player_walkleft");

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        // setup footstep audio source (use inspector slot if provided, otherwise create a child source)
        if (footstepSource == null)
        {
            var go = new GameObject("FootstepSource");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            footstepSource = go.AddComponent<AudioSource>();
        }
        footstepSource.playOnAwake = false;
        footstepSource.loop = true;
        footstepSource.volume = footstepVolume;
        // prefer explicit loop clip, otherwise fall back to first array element
        if (footstepLoopClip != null)
            footstepSource.clip = footstepLoopClip;
        else if (footstepClips != null && footstepClips.Length > 0)
            footstepSource.clip = footstepClips[0];
        if (footstepSource.isPlaying) footstepSource.Stop();

        // quick bind check
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            Debug.Log("[PlayerController] Animator bound. controller=" + (animator.runtimeAnimatorController != null));
        }

        // --- physics sanity checks / auto-fix (logging) ---
        if (rb == null)
        {
            Debug.LogWarning("[PlayerController] Rigidbody2D not found on Player.");
        }
        else
        {
            Debug.Log($"[PlayerController] Rigidbody2D bodyType={rb.bodyType} collisionDetection={rb.collisionDetectionMode} gravity={rb.gravityScale}");
            // optional: enforce Dynamic + continuous collision for fast movement
            if (rb.bodyType != RigidbodyType2D.Dynamic)
            {
                Debug.LogWarning("[PlayerController] Rigidbody2D is not Dynamic. Changing to Dynamic for normal collisions.");
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        var col = GetComponent<Collider2D>();
        if (col == null)
            Debug.LogWarning("[PlayerController] Collider2D not found on Player.");
        else
        {
            Debug.Log($"[PlayerController] Collider2D found. isTrigger={col.isTrigger} usedByComposite={(col is CompositeCollider2D)}");
            if (col.isTrigger)
                Debug.LogWarning("[PlayerController] Player collider is set as Trigger — collisions won't block movement.");
        }
    }

    // debug collision callbacks to verify what collides
    private void OnCollisionEnter2D(Collision2D c)
    {
        Debug.Log($"[PlayerController] OnCollisionEnter2D with '{c.collider.name}' layer={LayerMask.LayerToName(c.collider.gameObject.layer)}");
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        Debug.Log($"[PlayerController] OnTriggerEnter2D with '{c.name}' layer={LayerMask.LayerToName(c.gameObject.layer)} (player collider may be Trigger)");
    }

    void Update()
    {
        // read input (kamu masih bisa pake PlayerInput/OnMove jika aktif)
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        // determine whether should play footsteps
        bool shouldPlayFootsteps = movementInput.sqrMagnitude > 0.01f && CanMove && !isAttacking;
        HandleFootsteps(shouldPlayFootsteps);
        
        // animator hanya peduli apakah ada input bergerak (jangan ikat ke isAttacking)
        bool animatorIsMoving = movementInput.sqrMagnitude > 0.01f;
        if (animatorIsMoving)
            lastMoveDirection = movementInput.normalized;

        // send animator params (isMoving untuk transisi animasi WALK)
        if (animator != null)
        {
            animator.SetBool(PARAM_IS_MOVING, animatorIsMoving);
            animator.SetFloat(PARAM_MOVE_X, lastMoveDirection.x);
            animator.SetFloat(PARAM_MOVE_Y, lastMoveDirection.y);

            // determine integer direction for state machine (0=down,1=up,2=right,3=left)
            int moveDir;
            if (Mathf.Abs(lastMoveDirection.x) > Mathf.Abs(lastMoveDirection.y))
                moveDir = lastMoveDirection.x > 0 ? 2 : 3;
            else
                moveDir = lastMoveDirection.y > 0 ? 1 : 0;
            animator.SetInteger("MoveDir", moveDir);

            // hitung Direction (1=Left,2=Right,3=Up,4=Down)
            Vector2 dir = lastMoveDirection;
            int direction;
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                direction = dir.x > 0 ? 2 : 1; // kanan=2, kiri=1
            else
                direction = dir.y > 0 ? 3 : 4; // atas=3, bawah=4
            animator.SetInteger("Direction", direction);
            animator.SetBool("isMoving", movementInput.sqrMagnitude > 0.01f);

            if (HasAnimatorParameter(animator, "Blend"))
                animator.SetFloat("Blend", lastMoveDirection.x);

            // immediate state switch when direction changes while walking (but not while attacking)
            if (animatorIsMoving && !isAttacking && moveDir != prevMoveDir)
            {
                int targetHash = HASH_WALK_DOWN;
                switch (moveDir)
                {
                    case 0: targetHash = HASH_WALK_DOWN; break;
                    case 1: targetHash = HASH_WALK_UP; break;
                    case 2: targetHash = HASH_WALK_RIGHT; break;
                    case 3: targetHash = HASH_WALK_LEFT; break;
                }

                // crossfade with zero duration forces instant change between walk states
                animator.CrossFade(targetHash, 0f, 0, 0f);
                prevMoveDir = moveDir;
            }

            // reset prevMoveDir when stopped so next start will force state
            if (!animatorIsMoving) prevMoveDir = -1;
        }

        // actual movement only allowed when not attacking and movement is enabled
        // (movement performed in FixedUpdate to keep physics stable)
        // attack input
        if (Input.GetMouseButtonDown(0))
            OnAttack();

        // interaction: tekan 'E' untuk interaksi / memunculkan dialog
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Interactable != null)
            {
                // call Interact with the player so DialogueActivator can show dialogue
                Interactable.Interact(this);
            }
            else if (dialogueUI != null)
            {
                // fallback: try to open dialogue UI via SendMessage if available
                dialogueUI.gameObject.SendMessage("OpenDialogue", SendMessageOptions.DontRequireReceiver);
            }
        }

        // debug singkat (hapus saat sudah ok)
        if (Time.frameCount % 120 == 0) // log tiap ~2 detik untuk tidak spam console
        {
            Debug.Log($"[PlayerController] isAttacking={isAttacking} animatorIsMoving={animatorIsMoving} moveInput={movementInput} lastDir={lastMoveDirection}");
            if (animator != null)
            {
                var st = animator.GetCurrentAnimatorStateInfo(0);
                Debug.Log($"[Animator] stateHash={st.shortNameHash} inTransition={animator.IsInTransition(0)}");
            }
        }
    }

    void FixedUpdate()
    {
        // compute expected velocity from input (same logic you already use)
        Vector2 expectedVelocity = Vector2.zero;
        if (CanMove && !isAttacking && movementInput != Vector2.zero)
            expectedVelocity = movementInput.normalized * moveSpeed;

        // if you use MovePosition as before, keep MovePosition for expected movement
        if (expectedVelocity != Vector2.zero)
        {
            rb.MovePosition(rb.position + expectedVelocity * Time.fixedDeltaTime);
        }

        // --- protection: detect external/vertical pull and clamp ---
        Vector2 actualVel = rb.linearVelocity;
        float diff = (actualVel - expectedVelocity).magnitude;

        // threshold tweakable: if external influence larger than expected, log & correct
        const float EXTERNAL_PULL_THRESHOLD = 0.5f;
        if (diff > EXTERNAL_PULL_THRESHOLD && expectedVelocity.magnitude < 0.01f)
        {
            Debug.LogWarning($"[PlayerController] Unexpected external velocity detected. actual={actualVel} expected={expectedVelocity} pos={rb.position}");
            // optional: reset velocity to zero to stop pulling
            rb.linearVelocity = Vector2.zero;

            // also unparent if some code parented the player unexpectedly
            if (transform.parent != null)
            {
                Debug.LogWarning($"[PlayerController] Removing unexpected parent '{transform.parent.name}'");
                transform.SetParent(null);
            }
        }
        else if (diff > EXTERNAL_PULL_THRESHOLD)
        {
            Debug.LogWarning($"[PlayerController] External force while moving. actual={actualVel} expected={expectedVelocity}");
            // align velocity to expected to avoid being wrestled by other forces
            rb.linearVelocity = expectedVelocity;
        }

        // existing debug (optional)
        if (Time.frameCount % 120 == 0)
            Debug.Log($"[PlayerController] rb.gravityScale={rb.gravityScale} rb.bodyType={rb.bodyType} rb.velocity={rb.linearVelocity}");
    }

    void OnAttack()
    {
        if (!attackEnabled || isAttacking) return;
        isAttacking = true;

        if (attackHitboxes != null)
            attackHitboxes.SetAttackDirection(lastMoveDirection);

        int attackDir = 0;
        if (Mathf.Abs(lastMoveDirection.x) > Mathf.Abs(lastMoveDirection.y))
            attackDir = lastMoveDirection.x > 0 ? 1 : 2; // 1 = kanan, 2 = kiri
        else
            attackDir = lastMoveDirection.y > 0 ? 3 : 4; // 3 = atas, 4 = bawah

        if (animator != null)
        {
            animator.SetInteger(PARAM_ATTACK_DIR, attackDir);
            if (HasAnimatorParameter(animator, PARAM_ATTACK))
            {
                animator.SetTrigger(PARAM_ATTACK);
            }
            else
            {
                Debug.LogWarning("[PlayerController] Animator missing Trigger '" + PARAM_ATTACK + "'");
            }
        }

        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackDelay(attackDuration));
    }

    private IEnumerator AttackDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        isAttacking = false;
    }

    private void HandleFootsteps(bool play)
    {
        // guard: need a valid source and clip
        if (footstepSource == null || footstepSource.clip == null) return;

        // never play footsteps while attacking or movement locked
        if (isAttacking || !CanMove) play = false;

        if (play)
        {
            if (!footstepSource.isPlaying)
                footstepSource.Play();
        }
        else
        {
            if (footstepSource.isPlaying)
                footstepSource.Stop();
        }
    }

    private bool HasAnimatorParameter(Animator animator, string paramName)
    {
        // check using AnimatorControllerParameter (more reliable)
        foreach (var p in animator.parameters)
        {
            if (p.name == paramName)
                return true;
        }

        return false;
    }

    // --- optional: expose Rigidbody2D for external control (e.g. AI, events) ---
    public Rigidbody2D Rigidbody => rb;
     public void LockMovement()
   {
        CanMove = false;
        movementInput = Vector2.zero;
        if (footstepSource != null && footstepSource.isPlaying) footstepSource.Stop();
        if (animator != null) animator.SetBool(PARAM_IS_MOVING, false);
        if (rb != null) SetRigidbodyLinearVelocity(rb, Vector2.zero);

    }
    public void UnlockMovement()
    {
        CanMove = true;
    }

    private void SetRigidbodyLinearVelocity(Rigidbody2D rigidbody, Vector2 velocity)
    {
        // Use reflection to access the private field 'm_Velocity'
        FieldInfo velocityField = typeof(Rigidbody2D).GetField("m_Velocity", BindingFlags.NonPublic | BindingFlags.Instance);
        if (velocityField != null)
        {
            velocityField.SetValue(rigidbody, velocity);
        }
        else
        {
            Debug.LogWarning("Unable to access Rigidbody2D.m_Velocity field via reflection.");
        }
    }
}
