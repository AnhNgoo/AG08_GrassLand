using UnityEngine;

public class GrassBursh : MonoBehaviour
{
    [SerializeField] private int healthChangeAmount = 10; // Số máu thay đổi (cộng hoặc trừ)
    private bool hasBeenUsed = false; // Đánh dấu bụi cỏ đã được sử dụng

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra nếu đối tượng va chạm là player và bụi cỏ chưa được sử dụng
        if (other.CompareTag("Player") && !hasBeenUsed)
        {
            // Lấy component PlayerController từ player
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Random giữa cộng hoặc trừ máu
                bool isHealing = Random.Range(0, 2) == 0; // 50% cơ hội cộng hoặc trừ
                int healthChange = isHealing ? healthChangeAmount : -healthChangeAmount;

                // Áp dụng thay đổi máu cho player
                if (isHealing)
                {
                    // Cộng máu (cần thêm phương thức AddHealth vào PlayerController)
                    player.AddHealth(healthChange);
                    Debug.Log("Player gained " + healthChange + " health from grass bush!");
                }
                else
                {
                    // Trừ máu (sử dụng TakeDamage, nhưng không áp dụng knockback)
                    Vector2 noKnockbackDirection = Vector2.zero; // Không áp dụng knockback
                    player.TakeDamage(healthChangeAmount, noKnockbackDirection);
                    Debug.Log("Player lost " + healthChangeAmount + " health from grass bush!");
                }

                // Đánh dấu bụi cỏ đã được sử dụng
                hasBeenUsed = true;
            }
        }
    }
}