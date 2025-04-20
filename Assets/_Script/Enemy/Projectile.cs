using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f; // Tốc độ đạn
    public float lifetime = 3f; // Thời gian tồn tại của đạn
    private int damage; // Sát thương của đạn
    private Vector2 direction; // Hướng di chuyển của đạn
    private Rigidbody2D rb; // Rigidbody2D của đạn

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); // Xóa đạn sau lifetime
    }

    void FixedUpdate()
    {
        // Di chuyển đạn theo hướng
        rb.linearVelocity = direction * speed;
    }

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    public void SetDirection(Vector2 direction)
    {
        this.direction = direction.normalized;
        // Xoay sprite của đạn theo hướng
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Nếu đạn trúng player, gây sát thương
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                playerController.TakeDamage(damage, knockbackDirection);
            }
            Destroy(gameObject); // Xóa đạn khi trúng
        }
        else if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject); // Xóa đạn khi trúng chướng ngại vật
        }
    }
}