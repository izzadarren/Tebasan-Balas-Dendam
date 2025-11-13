using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float aggroRange = 6f;
    [SerializeField] private float stopDistance = 0.8f;
    [SerializeField] private bool kinematicMovement = true;

    [Header("Animator parameter names (match PlayerController)")]
    [SerializeField] private string paramMoveX = "MoveX";
    [SerializeField] private string paramMoveY = "MoveY";
    [SerializeField] private string paramIsMoving = "isMoving";
    [SerializeField] private string paramMoveDir = "MoveDir"; // 1=Up,2=Down,3=Left,4=Right
    [SerializeField] private string paramDirection = "Direction"; // 1=Left,2=Right,3=Up,4=Down

    [Header("Options")]
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private Transform playerTransform;

    private Rigidbody2D rb;
    private Animator animator;

    // movement state
    private Vector2 lastMoveDirection = Vector2.down;
    private Vector2 lastMoveVelocity = Vector2.zero;
    private bool isChasing = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (rb == null) Debug.LogError("[EnemyController] Rigidbody2D missing on " + name);
    }

    private void Start()
    {
        if (playerTransform == null && autoFindPlayer)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        if (animator != null)
        {
            // validate parameters (warning only)
            var required = new string[] { paramMoveX, paramMoveY, paramIsMoving, paramMoveDir, paramDirection };
            var existing = new HashSet<string>();
            foreach (var p in animator.parameters) existing.Add(p.name);
            foreach (var r in required)
                if (!existing.Contains(r))
                    Debug.LogWarning($"[EnemyController] Animator is missing parameter '{r}' on {name}. Parameter names are case-sensitive.");
        }

        if (rb != null && kinematicMovement)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        Vector2 velocity = Vector2.zero;

        if (playerTransform != null)
        {
            float dist = Vector2.Distance(playerTransform.position, rb.position);

            if (!isChasing && dist <= aggroRange) isChasing = true;
            else if (isChasing && dist > aggroRange * 1.5f) isChasing = false;

            if (isChasing)
            {
                Vector2 toPlayer = ((Vector2)playerTransform.position - rb.position);
                float d = toPlayer.magnitude;

                if (d > stopDistance)
                    velocity = toPlayer.normalized * ((chaseSpeed > 0f) ? chaseSpeed : moveSpeed);
                else
                {
                    velocity = Vector2.zero;
                    // face player while stopped
                    lastMoveDirection = (toPlayer.sqrMagnitude > 0.001f) ? toPlayer.normalized : lastMoveDirection;
                }
            }
        }

        // update direction memory (only update when moving to avoid jitter)
        if (velocity.sqrMagnitude > 0.0001f)
            lastMoveDirection = velocity.normalized;

        lastMoveVelocity = velocity;

        // apply movement (use helper to set linear velocity compatible across Unity versions)
        if (kinematicMovement)
        {
            if (velocity != Vector2.zero)
                rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            else
                Rigidbody2DExtensions.SetLinearVelocity(rb, Vector2.zero);
        }
        else
        {
            if (velocity != Vector2.zero)
                rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            else
                Rigidbody2DExtensions.SetLinearVelocity(rb, Vector2.zero);
        }
    }

    private void Update()
    {
        UpdateAnimationLikePlayer();
    }

    private void UpdateAnimationLikePlayer()
    {
        if (animator == null) return;

        bool animatorIsMoving = lastMoveVelocity.sqrMagnitude > 0.01f;

        // set basic movement params that AnyState transitions can use
        animator.SetBool(paramIsMoving, animatorIsMoving);
        animator.SetFloat(paramMoveX, lastMoveDirection.x);
        animator.SetFloat(paramMoveY, lastMoveDirection.y);

        // compute MoveDir with requested mapping:
        // 1 = Up, 2 = Down, 3 = Left, 4 = Right
        int moveDir;
        if (Mathf.Abs(lastMoveDirection.x) > Mathf.Abs(lastMoveDirection.y))
            moveDir = lastMoveDirection.x > 0 ? 4 : 3; // right=4, left=3
        else
            moveDir = lastMoveDirection.y > 0 ? 1 : 2; // up=1, down=2
        animator.SetInteger(paramMoveDir, moveDir);

        // compute Direction int (1=Left,2=Right,3=Up,4=Down) - unchanged
        int direction;
        Vector2 dirv = lastMoveDirection;
        if (Mathf.Abs(dirv.x) > Mathf.Abs(dirv.y))
            direction = dirv.x > 0 ? 2 : 1;
        else
            direction = dirv.y > 0 ? 3 : 4;
        animator.SetInteger(paramDirection, direction);

        // debug to help you tune thresholds (remove in production)
        // Debug.Log($"[EnemyController] isMoving={animatorIsMoving} MoveDir={moveDir} Direction={direction} MoveX={dirv.x:F2} MoveY={dirv.y:F2}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (rb != null) Gizmos.DrawWireSphere(rb.position, aggroRange);
        else Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}

// helper: safe linear velocity get/set compatible with 'velocity' or 'linearVelocity' API
static class Rigidbody2DExtensions
{
    private static readonly PropertyInfo propLinearVelocity;
    private static readonly PropertyInfo propVelocity;
    private static readonly Action<Rigidbody2D, Vector2> setter;
    private static readonly Func<Rigidbody2D, Vector2> getter;

    static Rigidbody2DExtensions()
    {
        var type = typeof(Rigidbody2D);
        // try linearVelocity first (newer API), then velocity (older API)
        propLinearVelocity = type.GetProperty("linearVelocity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        propVelocity = type.GetProperty("velocity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (propLinearVelocity != null && propLinearVelocity.PropertyType == typeof(Vector2) && propLinearVelocity.CanRead && propLinearVelocity.CanWrite)
        {
            setter = (rb, v) => propLinearVelocity.SetValue(rb, v, null);
            getter = (rb) => (Vector2)propLinearVelocity.GetValue(rb, null);
        }
        else if (propVelocity != null && propVelocity.PropertyType == typeof(Vector2) && propVelocity.CanRead && propVelocity.CanWrite)
        {
            setter = (rb, v) => propVelocity.SetValue(rb, v, null);
            getter = (rb) => (Vector2)propVelocity.GetValue(rb, null);
        }
        else
        {
            // fallback to using MovePosition zeroing (no direct velocity available)
            setter = (rb, v) =>
            {
                // nothing to set; as fallback ensure no movement by not setting velocity
                // (we can't modify private internals safely)
            };
            getter = (rb) => Vector2.zero;
        }
    }

    public static void SetLinearVelocity(Rigidbody2D rb, Vector2 v)
    {
        try { setter?.Invoke(rb, v); } catch { /* ignore reflection errors */ }
    }

    public static Vector2 GetLinearVelocity(Rigidbody2D rb)
    {
        try { return getter != null ? getter.Invoke(rb) : Vector2.zero; } catch { return Vector2.zero; }
    }
}
