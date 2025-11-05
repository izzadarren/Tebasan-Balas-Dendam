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
    
    Vector2 movementInput;
    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    public bool CanMove = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

    }
    void Update()
    {
        if (dialogueUI.IsOpen) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"E pressed. Interactable={(Interactable != null ? Interactable.GetType().Name : "null")}, DialogueOpen={(dialogueUI != null ? dialogueUI.IsOpen.ToString() : "null")}");
            // panggil interact hanya jika ada Interactable dan dialog belum terbuka
            if (Interactable != null && (dialogueUI == null || !dialogueUI.IsOpen))
            {
                Interactable.Interact(this);
            }
        }
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        if (CanMove)
        {
            if (movementInput != Vector2.zero)
            {
                bool success = TryMove(movementInput);
                if (!success && movementInput.x > 0)
                {
                    success = TryMove(new Vector2(movementInput.x, 0));
                }
                if (!success && movementInput.y < 0)
                {
                    success = TryMove(new Vector2(0, movementInput.y));
                }

                animator.SetBool("IsMoving", success);
            }
            else
            {
                animator.SetBool("IsMoving", false);
            }

            if (movementInput.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (movementInput.x > 0)
            {
                spriteRenderer.flipX = false;
            }
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
        animator.SetTrigger("swordAttack");
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
