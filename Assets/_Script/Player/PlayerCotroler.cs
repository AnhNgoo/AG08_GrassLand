using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public GameObject enemyList;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int AttackIndexParam = Animator.StringToHash("AttackIndex");
    private static readonly int HurtTrigger = Animator.StringToHash("Hurt");
    private static readonly int DieTrigger = Animator.StringToHash("Die");

    private Color originalColor;
    public float invincibilityTime = 1f;
    private bool isInvincible = false;

    // Knockback system
    [Header("Knockback Settings")]
    [SerializeField] private float KnockbackForce = 0f;
    [SerializeField] private float durPushBack = 0.2f; // Thời gian đẩy lùi
    private Vector2 pushBackVelocity; // Vận tốc đẩy lùi
    private bool isPushBack = false; // Trạng thái đẩy lùi

    [Header("Run SFX Settings")]
    [SerializeField] private float runSoundInterval = 0.2f; // Khoảng thời gian giữa các lần phát RunSFX
    private bool wasMovingLastFrame = false; // Trạng thái di chuyển ở frame trước
    private float lastRunSoundTime; // Thời gian lần cuối phát RunSFX
    private bool canMove = true;

    [Header("Scroll Collection")]
    [SerializeField] private GameObject scrollList;

    [Header("UI Manager")]
    [SerializeField] private UIManager uiManager;

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
       if (isDead || !canMove) return;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        bool isMoving = movement != Vector2.zero;
        animator.SetBool(IsMoving, isMoving);

        // Phát RunSFX khi bắt đầu di chuyển và đủ thời gian interval
        if (isMoving && !wasMovingLastFrame && !isAttacking && !isPushBack)
        {
            if (Time.time >= lastRunSoundTime + runSoundInterval)
            {
                AudioManager.Instance.PlayRunSFX();
                lastRunSoundTime = Time.time;
            }
        }
        else if (isMoving && Time.time >= lastRunSoundTime + runSoundInterval && !isAttacking && !isPushBack)
        {
            AudioManager.Instance.PlayRunSFX();
            lastRunSoundTime = Time.time;
        }

        wasMovingLastFrame = isMoving;

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
        if (isDead || !canMove) return;

        // Ưu tiên áp dụng knockback nếu đang trong trạng thái đẩy lùi
        if (isPushBack)
        {
            rb.linearVelocity = pushBackVelocity;
            return;
        }

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
        AudioManager.Instance.PlayAttackSFX();

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

        // Chỉ áp dụng knockback nếu không đang tấn công
        if (!isAttacking)
        {
            _PushBack(knockbackDirection);
            CameraShake.ins.Shake(1, 20, 0.3f);
        }

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
            AudioManager.Instance.PlayHitSFX();
        }

        Invoke("ResetInvincibility", invincibilityTime);
    }

    public void AddHealth(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth; // Đảm bảo máu không vượt quá maxHealth
        }

        Debug.Log("Player health updated: " + currentHealth);
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
        AudioManager.Instance.PlayDieSFX();
        animator.SetTrigger(DieTrigger);
        rb.linearVelocity = Vector2.zero;
    }

    public void OnDieAnimationEnd()
    {
        uiManager.ShowGameOver();
    }

    public bool IsDie()
    {
        return isDead;
    }

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero; // Dừng di chuyển ngay lập tức
            movement = Vector2.zero; // Đặt movement về 0 để không di chuyển
            animator.SetBool(IsMoving, false); // Dừng animation di chuyển
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Win"))
        {
            // Kiểm tra xem ScrollList còn object con nào không
            if (scrollList != null && scrollList.transform.childCount == 0 && enemyList.transform.childCount == 0)
            {
                Debug.Log("You win! All scrolls collected!");
                SceneManager.LoadScene("GrassLand_Map2");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector2 attackDirection = spriteRenderer != null && spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 attackPosition = (Vector2)transform.position + attackDirection * 0.5f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPosition, attackRange);
    }

    // Knockback system
    private void _PushBack(Vector2 direction)
    {
        pushBackVelocity = direction.normalized * KnockbackForce;
        isPushBack = true;
        StartCoroutine(PushBacking());
    }

    private IEnumerator PushBacking()
    {
        yield return new WaitForSeconds(durPushBack);
        pushBackVelocity = Vector2.zero;
        isPushBack = false;
    }
}