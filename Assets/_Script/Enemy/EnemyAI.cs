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

    [Header("Patrol Settings")]
    public float patrolRadius = 5f; // Bán kính vùng giới hạn di chuyển

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
    public GameObject deathEffect;

    [Header("Avoidance Settings")]
    public float maxAvoidanceTime = 5f;

    private Transform player;
    private Vector2 randomDirection;
    private Vector2 initialPosition; // Vị trí trung tâm của vùng giới hạn
    private bool isWandering = false;
    private bool isChasing = false;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isReturningToPatrol = false; // Trạng thái quay lại vùng giới hạn
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float lastAttackTime;
    private Color originalColor;
    private float avoidanceTimer = 0f;

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
        initialPosition = transform.position; // Lưu vị trí ban đầu của enemy

        StartCoroutine(RandomMovement());
    }

    void Update()
    {
        if (isDead || player == null) return;

        animator?.SetBool(IsMoving, rb.linearVelocity.magnitude > 0.1f);

        if (CanDetectPlayer())
        {
            isChasing = true;
            isWandering = false;
            ChasePlayer();

            if (Vector2.Distance(transform.position, player.position) <= attackRange &&
                Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
        else
        {
            // Khi player ra khỏi tầm nhìn, cập nhật initialPosition và quay lại di chuyển ngẫu nhiên
            if (isChasing)
            {
                initialPosition = transform.position; // Cập nhật vị trí trung tâm mới
                isChasing = false;
            }

            // Kiểm tra vùng giới hạn chỉ khi không đuổi theo player
            if (!isChasing && Vector2.Distance(transform.position, initialPosition) > patrolRadius)
            {
                isWandering = false;
                isReturningToPatrol = true;
                ReturnToPatrolArea();
            }
            else
            {
                isReturningToPatrol = false;
                if (!isWandering && !isAttacking)
                {
                    StartCoroutine(RandomMovement());
                }
            }
        }
    }

    bool CanDetectPlayer()
    {
        return Vector2.Distance(transform.position, player.position) <= detectionRadius;
    }

    void ChasePlayer()
    {
        if (isAttacking) return;

        Vector2 directPath = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directPath, 1f, obstacleLayer);

        if (hit.collider != null)
        {
            avoidanceTimer += Time.deltaTime;
            if (avoidanceTimer >= maxAvoidanceTime)
            {
                isChasing = false;
                avoidanceTimer = 0f;
                return;
            }

            Vector2 avoidanceDirection = FindAvoidanceDirection(directPath);
            rb.linearVelocity = avoidanceDirection * chaseSpeed;
            UpdateFacingDirection(avoidanceDirection);
        }
        else
        {
            avoidanceTimer = 0f;
            rb.linearVelocity = directPath * chaseSpeed;
            UpdateFacingDirection(directPath);
        }
    }

    void ReturnToPatrolArea()
    {
        Vector2 directionToInitial = (initialPosition - (Vector2)transform.position).normalized;
        rb.linearVelocity = directionToInitial * moveSpeed;
        UpdateFacingDirection(directionToInitial);
    }

    Vector2 FindAvoidanceDirection(Vector2 directPath)
    {
        Vector2 rightPerpendicular = new Vector2(directPath.y, -directPath.x).normalized;
        Vector2 leftPerpendicular = new Vector2(-directPath.y, directPath.x).normalized;

        RaycastHit2D rightHit = Physics2D.Raycast(transform.position, rightPerpendicular, 1f, obstacleLayer);
        if (rightHit.collider == null)
        {
            return rightPerpendicular;
        }

        RaycastHit2D leftHit = Physics2D.Raycast(transform.position, leftPerpendicular, 1f, obstacleLayer);
        if (leftHit.collider == null)
        {
            return leftPerpendicular;
        }

        float rightDistance = rightHit.collider != null ? rightHit.distance : float.MaxValue;
        float leftDistance = leftHit.collider != null ? leftHit.distance : float.MaxValue;

        if (rightDistance > leftDistance)
        {
            return rightPerpendicular;
        }
        else
        {
            return leftPerpendicular;
        }
    }

    void AttackPlayer()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetTrigger(AttackTrigger);
        }

        bool canAttack = player != null && Vector2.Distance(transform.position, player.position) <= attackRange;
        Vector2 knockbackDirection = (player.position - transform.position).normalized;
        StartCoroutine(DealDamageAfterDelay(0.3f, canAttack, knockbackDirection));
    }

    IEnumerator DealDamageAfterDelay(float delay, bool canAttack, Vector2 knockbackDirection)
    {
        yield return new WaitForSeconds(delay);

        if (isDead) yield break;

        if (canAttack)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(attackDamage, knockbackDirection);
            }
        }

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    IEnumerator RandomMovement()
    {
        isWandering = true;

        while (!isChasing && !isDead && !isReturningToPatrol)
        {
            randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

            Vector2 nextPosition = (Vector2)transform.position + randomDirection * moveSpeed * Time.deltaTime;
            if (Vector2.Distance(nextPosition, initialPosition) > patrolRadius)
            {
                randomDirection = (initialPosition - (Vector2)transform.position).normalized;
            }

            RaycastHit2D initialHit = Physics2D.Raycast(transform.position, randomDirection, 1f, obstacleLayer);
            if (initialHit.collider != null)
            {
                randomDirection = FindAvoidanceDirection(randomDirection);
            }

            float wanderTime = Random.Range(minWanderTime, maxWanderTime);
            float timer = 0;

            while (timer < wanderTime && !isChasing && !isDead && !isReturningToPatrol)
            {
                if (!isAttacking)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, randomDirection, 1f, obstacleLayer);
                    if (hit.collider != null)
                    {
                        randomDirection = FindAvoidanceDirection(randomDirection);
                    }

                    nextPosition = (Vector2)transform.position + randomDirection * moveSpeed * Time.deltaTime;
                    if (Vector2.Distance(nextPosition, initialPosition) > patrolRadius)
                    {
                        randomDirection = (initialPosition - (Vector2)transform.position).normalized;
                    }

                    rb.linearVelocity = randomDirection * moveSpeed;
                    UpdateFacingDirection(randomDirection);
                }
                timer += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(waitBetweenWanderTime);

            if (isChasing || isDead || isReturningToPatrol) break;
        }

        isWandering = false;
    }

    void UpdateFacingDirection(Vector2 direction)
    {
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        StartCoroutine(FlashEffect());

        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        CameraShake.ins.Shake(1, 20, 0.3f);

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (animator != null)
        {
            animator.SetTrigger(HurtTrigger);
            StartCoroutine(PauseAfterHit());
        }
    }

    IEnumerator PauseAfterHit()
    {
        isAttacking = true;
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

        if (animator != null)
        {
            animator.SetTrigger(DieTrigger);
        }

        rb.linearVelocity = Vector2.zero;

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D c in colliders)
        {
            c.enabled = false;
        }

        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.4f);
        }
        StartCoroutine(DestroyAfterDelay(1.5f));
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(initialPosition, patrolRadius);
    }
}