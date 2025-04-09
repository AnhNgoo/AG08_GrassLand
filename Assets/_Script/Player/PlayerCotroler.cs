using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    public float moveSpeed = 5f;
    private Vector2 movement;

    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    public int attackDamage = 25;
    public float attackRange = 1.2f;
    private int attackIndex = 0;
    private bool isAttacking = false;
    private float attackCooldown = 0.5f;
    private float lastAttackTime;
    public LayerMask enemyLayer;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int AttackIndexParam = Animator.StringToHash("AttackIndex");
    private static readonly int HurtTrigger = Animator.StringToHash("Hurt");
    private static readonly int DieTrigger = Animator.StringToHash("Die");

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
        lastAttackTime = -attackCooldown;
        originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (isDead) return;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        animator.SetBool(IsMoving, movement != Vector2.zero);

        if (movement.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (movement.x > 0)
        {
            spriteRenderer.flipX = false;
        }

        if ((Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0)) && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(20, Random.insideUnitCircle.normalized);
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero; // Dừng hoàn toàn chuyển động khi đang tấn công
            return;
        }

        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void Attack()
    {
        if (isDead) return;

        isAttacking = true;
        lastAttackTime = Time.time;
        rb.linearVelocity = Vector2.zero;

        attackIndex = (attackIndex + 1) % 3;
        animator.SetInteger(AttackIndexParam, attackIndex);
        animator.SetTrigger(AttackTrigger);

        Invoke("DealDamage", 0.2f);
    }

    void DealDamage()
    {
        Vector2 attackDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 attackPosition = (Vector2)transform.position + attackDirection * 0.5f;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPosition, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemyAI.TakeDamage(attackDamage, knockbackDir);
            }
        }
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isDead) return;

        // Áp dụng knockback ngay cả khi đang bất tử
        //rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // Chỉ nhận sát thương nếu không đang bất tử
        if (isInvincible) return;

        currentHealth -= damage;
        isInvincible = true;

        StartCoroutine(FlashEffect());

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            animator.SetTrigger(HurtTrigger);
        }

        Invoke("ResetInvincibility", invincibilityTime);
    }

    void ResetInvincibility()
    {
        isInvincible = false;
    }

    IEnumerator FlashEffect()
    {
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
        rb.linearVelocity = Vector2.zero;
    }

    public void OnDieAnimationEnd()
    {
        Time.timeScale = 0f;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    void OnDrawGizmosSelected()
    {
        Vector2 attackDirection = spriteRenderer != null && spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 attackPosition = (Vector2)transform.position + attackDirection * 0.5f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPosition, attackRange);
    }
}