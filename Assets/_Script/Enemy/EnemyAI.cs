using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float detectionRadius = 5f;
    public float chaseSpeed = 3f;
    public LayerMask obstacleLayer;
    public LayerMask playerLayer;
    
    [Header("Random Movement Settings")]
    public float minWanderTime = 1f;
    public float maxWanderTime = 3f;
    public float waitBetweenWanderTime = 1f;
    
    [Header("Combat Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public int attackDamage = 10;
    public float attackRange = 1f;
    public float attackCooldown = 1.5f;
    public float knockbackForce = 5f;
    public GameObject deathEffect; // Optional: Death effect prefab
    
    // Components
    private Transform player;
    private Vector2 randomDirection;
    private bool isWandering = false;
    private bool isChasing = false;
    private bool isAttacking = false;
    private bool isDead = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float lastAttackTime;
    private Color originalColor;
    
    // Animator Parameters
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int AttackTrigger = Animator.StringToHash("IsAttacking");
    private static readonly int HurtTrigger = Animator.StringToHash("IsHurting");
    private static readonly int DieTrigger = Animator.StringToHash("IsDead");
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        currentHealth = maxHealth;
        lastAttackTime = -attackCooldown;
        originalColor = spriteRenderer.color;
        
        // Bắt đầu hành vi di chuyển ngẫu nhiên
        StartCoroutine(RandomMovement());
    }
    
    void Update()
    {
        if (isDead || player == null) return;
        
        // Update animation based on movement
        animator?.SetBool(IsMoving, rb.linearVelocity.magnitude > 0.1f);
        
        // Kiểm tra xem có phát hiện player không
        if (CanDetectPlayer())
        {
            isChasing = true;
            isWandering = false;
            ChasePlayer();
            
            // Kiểm tra nếu đủ gần để tấn công
            if (Vector2.Distance(transform.position, player.position) <= attackRange && 
                Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
        else
        {
            isChasing = false;
            if (!isWandering && !isAttacking)
            {
                StartCoroutine(RandomMovement());
            }
        }
    }
    
    bool CanDetectPlayer()
    {
        // Kiểm tra xem player có nằm trong phạm vi phát hiện không
        if (Vector2.Distance(transform.position, player.position) <= detectionRadius)
        {
            // Kiểm tra xem có vật cản giữa enemy và player không
            RaycastHit2D hit = Physics2D.Linecast(transform.position, player.position, obstacleLayer);
            if (hit.collider == null)
            {
                return true; // Không có vật cản, có thể nhìn thấy player
            }
        }
        return false;
    }
    
    void ChasePlayer()
    {
        if (isAttacking) return;
        
        // Di chuyển về phía player
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * chaseSpeed;
        
        // Cập nhật hướng nhìn
        UpdateFacingDirection(direction);
    }
    
    void AttackPlayer()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        rb.linearVelocity = Vector2.zero; // Dừng di chuyển khi tấn công
        
        // Kích hoạt animation tấn công
        if (animator != null)
        {
            animator.SetTrigger(AttackTrigger);
        }
        
        // Sẽ thực hiện tấn công thực sự sau một khoảng delay để đồng bộ với animation
        StartCoroutine(DealDamageAfterDelay(0.3f));
    }
    
    IEnumerator DealDamageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (isDead) yield break;
        
        // Kiểm tra khoảng cách một lần nữa, đảm bảo player vẫn trong tầm đánh
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            // Gây sát thương cho player
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(attackDamage);
            }
        }
        
        // Đợi kết thúc animation tấn công
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }
    
    IEnumerator RandomMovement()
    {
        isWandering = true;
        
        while (!isChasing && !isDead)
        {
            // Chọn hướng ngẫu nhiên
            randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            
            // Kiểm tra xem hướng mới có va chạm với chướng ngại vật không
            RaycastHit2D hit = Physics2D.Raycast(transform.position, randomDirection, 1f, obstacleLayer);
            if (hit.collider != null)
            {
                // Nếu có vật cản, thử hướng khác
                yield return new WaitForSeconds(0.2f);
                continue;
            }
            
            // Di chuyển theo hướng đã chọn
            float wanderTime = Random.Range(minWanderTime, maxWanderTime);
            float timer = 0;
            
            while (timer < wanderTime && !isChasing && !isDead)
            {
                if (!isAttacking)
                {
                    rb.linearVelocity = randomDirection * moveSpeed;
                    UpdateFacingDirection(randomDirection);
                }
                timer += Time.deltaTime;
                yield return null;
            }
            
            // Dừng lại giữa các lần di chuyển
            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(waitBetweenWanderTime);
            
            // Kiểm tra nếu đã bắt đầu đuổi thì thoát
            if (isChasing || isDead) break;
        }
        
        isWandering = false;
    }
    
    void UpdateFacingDirection(Vector2 direction)
    {
        // Xử lý hướng nhìn (flip sprite nếu cần)
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
    
    // Xử lý khi nhận sát thương từ player
    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        // Đảm bảo máu không âm cho health bar
        if (currentHealth < 0)
            currentHealth = 0;
        
        // Hiệu ứng flash khi bị đánh
        StartCoroutine(FlashEffect());
        
        // Áp dụng lực đẩy
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else if (animator != null)
        {
            animator.SetTrigger(HurtTrigger);
            
            // Dừng hành động trong chốc lát khi bị đánh
            StartCoroutine(PauseAfterHit());
        }
    }
    
    IEnumerator PauseAfterHit()
    {
        isAttacking = true; // Sử dụng biến isAttacking để tạm ngưng chuyển động
        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }
    
    IEnumerator FlashEffect()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
    
    void Die()
    {
        isDead = true;
        
        // Trigger animation chết
        if (animator != null)
        {
            animator.SetTrigger(DieTrigger);
        }
        
        // Dừng mọi chuyển động
        rb.linearVelocity = Vector2.zero;
        
        // Vô hiệu hóa các collider nếu có
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D c in colliders)
        {
            c.enabled = false;
        }
        
        // Hiệu ứng chết nếu có
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Xóa game object sau khi animation chết kết thúc
        StartCoroutine(DestroyAfterDelay(1.5f));
    }
    
    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
    
    // Được gọi khi animation attack kết thúc (thông qua Animation Event)
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
    }
    
    // Hiển thị phạm vi phát hiện trong Editor để dễ tinh chỉnh
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}