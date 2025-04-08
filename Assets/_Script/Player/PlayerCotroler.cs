using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // Components
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Movement
    public float moveSpeed = 5f;
    private Vector2 movement; // Handle movement in all 4 directions

    // Health
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    // Attack
    public int attackDamage = 25;  // Damage per hit
    public float attackRange = 1.2f;  // Range of attack
    private int attackIndex = 0; // To cycle through Attack 1, 2, 3
    private bool isAttacking = false;
    private float attackCooldown = 0.5f; // Cooldown between attacks
    private float lastAttackTime;
    public LayerMask enemyLayer; // Enemy layer to attack

    // Animator Parameters
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int AttackIndexParam = Animator.StringToHash("AttackIndex");
    private static readonly int HurtTrigger = Animator.StringToHash("Hurt");
    private static readonly int DieTrigger = Animator.StringToHash("Die");

    // Feedback
    private Color originalColor;
    public float knockbackForce = 8f;
    public float invincibilityTime = 1f;
    private bool isInvincible = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        lastAttackTime = -attackCooldown; // Allow immediate attack at start
        originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (isDead) return; // Stop all actions if dead

        // Movement Input (4 directions)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Update Animator for movement
        // Only use horizontal movement (movement.x) to determine animation
        animator.SetBool(IsMoving, movement != Vector2.zero);

        // Flip sprite based on horizontal direction (Left/Right)
        if (movement.x < 0)
        {
            spriteRenderer.flipX = true; // Face left
        }
        else if (movement.x > 0)
        {
            spriteRenderer.flipX = false; // Face right
        }

        // Attack Input (J key or Left Mouse Button)
        if ((Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0)) && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }

        // For testing: Press H to simulate taking damage
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(20); // Test taking 20 damage
        }
    }

    void FixedUpdate()
    {
        if (isDead || isAttacking) return; // Don't move while attacking or dead

        // Move the player (4 directions)
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void Attack()
    {
        if (isDead) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        // Cycle through Attack animations (1 → 2 → 3 → 1)
        attackIndex = (attackIndex + 1) % 3; // 0, 1, 2, then loop back to 0
        animator.SetInteger(AttackIndexParam, attackIndex);
        animator.SetTrigger(AttackTrigger);

        // Will perform damage after a short delay to sync with animation
        Invoke("DealDamage", 0.2f);
    }

    void DealDamage()
    {
        // Determine attack direction based on player facing
        Vector2 attackDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 attackPosition = (Vector2)transform.position + attackDirection * 0.5f;

        // Detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPosition, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Get the enemy script and apply damage
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                // Direction for knockback (from player to enemy)
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemyAI.TakeDamage(attackDamage, knockbackDir);
            }
        }
    }

    // Called when attack animation ends (via Animation Event)
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        isInvincible = true;

        // Visual feedback
        StartCoroutine(FlashEffect());

        if (currentHealth <= 0)
        {
            currentHealth = 0; // Đảm bảo máu không âm cho health bar
            Die();
        }
        else
        {
            animator.SetTrigger(HurtTrigger);
            // Knockback when hit
            Vector2 knockbackDirection = rb.linearVelocity.normalized;
            if (knockbackDirection == Vector2.zero)
            {
                // If player is standing still, knockback backwards
                knockbackDirection = -movement.normalized;
                if (knockbackDirection == Vector2.zero)
                {
                    // Random direction if no movement input
                    knockbackDirection = Random.insideUnitCircle.normalized;
                }
            }
            rb.AddForce(-knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        // Reset invincibility after delay
        Invoke("ResetInvincibility", invincibilityTime);
    }

    void ResetInvincibility()
    {
        isInvincible = false;
    }

    IEnumerator FlashEffect()
    {
        // Flash the sprite red and then back to normal
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger(DieTrigger);
        rb.linearVelocity = Vector2.zero; // Stop movement
    }

    // Called when Die animation ends (via Animation Event)
    public void OnDieAnimationEnd()
    {
        // Freeze the game (you can also show a Game Over screen here)
        Time.timeScale = 0f;
    }

    // Method này được thêm để HealthBarManager có thể truy cập
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // Draw attack range in the editor (for debugging)
    void OnDrawGizmosSelected()
    {
        Vector2 attackDirection = transform.localScale.x < 0 ? Vector2.left : Vector2.right;
        Vector2 attackPosition = (Vector2)transform.position + attackDirection * 0.5f;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPosition, attackRange);
    }
}