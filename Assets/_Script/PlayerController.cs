using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Variables
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    private float moveInput;
    private bool isFacingRight = true;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    private bool isGrounded;

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Check if player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        anim.SetBool("isGrounded", isGrounded);

        // Handle input
        HandleMovement();
        HandleActions();

        // Check falling state
        HandleFalling();
    }

    void FixedUpdate()
    {
        // Apply movement
        float currentSpeed = Input.GetKey(KeyCode.LeftControl) ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
    }

    void HandleMovement()
    {
        // Get horizontal input
        moveInput = Input.GetAxisRaw("Horizontal");

        // Set animation parameters
        anim.SetFloat("speed", Mathf.Abs(moveInput));

        // Flip character
        if (moveInput > 0 && !isFacingRight)
            Flip();
        else if (moveInput < 0 && isFacingRight)
            Flip();

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("jump");
        }
    }

    void HandleActions()
    {
        // Attack
        if (Input.GetKeyDown(KeyCode.J))
        {
            anim.SetTrigger("attack");
        }

        // Block
        anim.SetBool("isBlocking", Input.GetKey(KeyCode.K));
    }

    void HandleFalling()
    {
        // Check if falling (not grounded and moving downward)
        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            anim.SetBool("isFalling", true);
        }
        else
        {
            anim.SetBool("isFalling", false);
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }
}