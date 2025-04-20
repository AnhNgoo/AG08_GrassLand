using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Điều khiển nhân vật người chơi
public class PlayerController : MonoBehaviour
{
    private Animator animator; // Thành phần animation
    private Rigidbody2D rb; // Thành phần vật lý
    private SpriteRenderer spriteRenderer; // Thành phần sprite

    public float moveSpeed = 5f; // Tốc độ di chuyển
    private Vector2 movement; // Vector hướng di chuyển

    public int maxHealth = 100; // Máu tối đa
    private int currentHealth; // Máu hiện tại
    private bool isDead = false; // Trạng thái chết

    public int attackDamage = 25; // Sát thương tấn công
    public float attackRange = 1.2f; // Phạm vi tấn công
    private int attackIndex = 0; // Chỉ số combo tấn công
    private bool isAttacking = false; // Đang tấn công?
    private float attackCooldown = 0.5f; // Thời gian chờ giữa các đòn
    private float lastAttackTime; // Thời điểm tấn công cuối
    public LayerMask enemyLayer; // Layer của enemy
    public GameObject enemyList; // Danh sách enemy

    private static readonly int IsMoving = Animator.StringToHash("IsMoving"); // ID animation di chuyển
    private static readonly int AttackTrigger = Animator.StringToHash("Attack"); // ID animation tấn công
    private static readonly int AttackIndexParam = Animator.StringToHash("AttackIndex"); // ID chỉ số combo
    private static readonly int HurtTrigger = Animator.StringToHash("Hurt"); // ID animation bị đau
    private static readonly int DieTrigger = Animator.StringToHash("Die"); // ID animation chết

    private Color originalColor; // Màu gốc của sprite
    public float invincibilityTime = 1f; // Thời gian bất tử
    private bool isInvincible = false; // Trạng thái bất tử

    [Header("Knockback Settings")]
    [SerializeField] private float KnockbackForce = 0f; // Lực đẩy lùi
    [SerializeField] private float durPushBack = 0.2f; // Thời gian đẩy lùi
    private Vector2 pushBackVelocity; // Vận tốc đẩy lùi
    private bool isPushBack = false; // Trạng thái đẩy lùi

    [Header("Run SFX Settings")]
    [SerializeField] private float runSoundInterval = 0.2f; // Khoảng cách phát âm thanh chạy
    private bool wasMovingLastFrame = false; // Đang di chuyển khung trước?
    private float lastRunSoundTime; // Thời điểm phát âm chạy cuối
    private bool canMove = true; // Có thể di chuyển?

    [Header("Scroll Collection")]
    [SerializeField] private GameObject scrollList; // Danh sách vật phẩm

    [Header("UI Manager")]
    [SerializeField] private UIManager uiManager; // Quản lý UI

    [Header("Mobile UI")]
    [SerializeField] private Joystick joystick; // Joystick cho mobile

    void Start()
    {
        // Khởi tạo thành phần
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        lastAttackTime = -attackCooldown;
        originalColor = spriteRenderer.color;
        if (uiManager != null)
            uiManager.SetAttackButtonListener(Attack); // Gán nút tấn công
    }

    void Update()
    {
        if (isDead || !canMove) return;

        bool isMobileUI = uiManager != null && uiManager.IsMobileUI();

        // Lấy input di chuyển
        if (isMobileUI && joystick != null)
            movement = new Vector2(joystick.Horizontal, joystick.Vertical);
        else
            movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        bool isMoving = movement != Vector2.zero;
        animator.SetBool(IsMoving, isMoving);

        // Phát âm thanh chạy
        if (isMoving && (!wasMovingLastFrame || Time.time >= lastRunSoundTime + runSoundInterval) && !isAttacking && !isPushBack)
        {
            AudioManager.Instance.PlayRunSFX();
            lastRunSoundTime = Time.time;
        }
        wasMovingLastFrame = isMoving;

        // Xoay sprite theo hướng
        if (movement.x < 0)
            spriteRenderer.flipX = true;
        else if (movement.x > 0)
            spriteRenderer.flipX = false;

        // Tấn công bằng phím/chuột
        if (!isMobileUI && (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0)) && Time.time >= lastAttackTime + attackCooldown)
            Attack();

        // Test nhận sát thương
        if (Input.GetKeyDown(KeyCode.H))
            TakeDamage(20, Random.insideUnitCircle.normalized);
    }

    void FixedUpdate()
    {
        if (isDead || !canMove) return;

        // Áp dụng đẩy lùi
        if (isPushBack)
        {
            rb.linearVelocity = pushBackVelocity;
            return;
        }

        // Dừng di chuyển khi tấn công
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Di chuyển nhân vật
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void Attack()
    {
        if (isDead) return;

        isAttacking = true;
        lastAttackTime = Time.time;
        rb.linearVelocity = Vector2.zero;
        AudioManager.Instance.PlayAttackSFX();

        attackIndex = (attackIndex + 1) % 3; // Chuyển đổi combo
        animator.SetInteger(AttackIndexParam, attackIndex);
        animator.SetTrigger(AttackTrigger); // Kích hoạt animation
        Invoke("DealDamage", 0.2f); // Gây sát thương sau delay
    }

    void DealDamage()
    {
        // Tấn công enemy trong phạm vi
        Vector2 attackDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 attackPosition = (Vector2)transform.position + attackDirection * 0.5f;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPosition, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemyAI.TakeDamage(attackDamage, knockbackDir); // Gây sát thương và đẩy lùi
            }

            BossAI bossAI = enemy.GetComponent<BossAI>();
            if (bossAI != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                bossAI.TakeDamage(attackDamage, knockbackDir);
            }
        }
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false; // Kết thúc trạng thái tấn công
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isDead || isInvincible) return;

        if (!isAttacking)
        {
            _PushBack(knockbackDirection); // Áp dụng đẩy lùi
            CameraShake.ins.Shake(1, 20, 0.3f); // Rung camera
        }

        currentHealth -= damage;
        isInvincible = true;
        StartCoroutine(FlashEffect()); // Hiệu ứng nhấp nháy

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            animator.SetTrigger(HurtTrigger); // Animation bị đau
            AudioManager.Instance.PlayHitSFX();
        }

        Invoke("ResetInvincibility", invincibilityTime); // Hết bất tử
    }

    public void AddHealth(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth); // Tăng máu
        StartCoroutine(HealEffect());
    }

    void ResetInvincibility()
    {
        isInvincible = false; // Kết thúc bất tử
    }

    IEnumerator FlashEffect()
    {
        // Hiệu ứng nhấp nháy khi bị đánh
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    IEnumerator HealEffect()
    {
         // Hiệu ứng nhấp nháy khi hồi máu
        spriteRenderer.color = Color.green;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.green;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        isDead = true;
        AudioManager.Instance.PlayDieSFX();
        animator.SetTrigger(DieTrigger); // Animation chết
        rb.linearVelocity = Vector2.zero;
    }

    public void OnDieAnimationEnd()
    {
        uiManager.ShowGameOver(); // Hiển thị game over
    }

    public bool IsDie()
    {
        return isDead; // Trả về trạng thái chết
    }

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            movement = Vector2.zero;
            animator.SetBool(IsMoving, false); // Dừng di chuyển
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth; // Trả về máu hiện tại
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra điều kiện thắng
        if (other.CompareTag("Win"))
        {
            if (scrollList != null && scrollList.transform.childCount == 0 && enemyList.transform.childCount == 0)
                if (uiManager != null)
                uiManager.ShowComingSoon(); // Hiển thị panel "Coming Soon"
        }
    }

    void OnDrawGizmosSelected()
    {
        // Vẽ phạm vi tấn công trong editor
        Vector2 attackDirection = spriteRenderer != null && spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 attackPosition = (Vector2)transform.position + attackDirection * 0.5f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPosition, attackRange);
    }

    private void _PushBack(Vector2 direction)
    {
        // Áp dụng đẩy lùi
        pushBackVelocity = direction.normalized * KnockbackForce;
        isPushBack = true;
        StartCoroutine(PushBacking());
    }

    private IEnumerator PushBacking()
    {
        yield return new WaitForSeconds(durPushBack);
        pushBackVelocity = Vector2.zero;
        isPushBack = false; // Kết thúc đẩy lùi
    }
}