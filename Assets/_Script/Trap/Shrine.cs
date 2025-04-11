using UnityEngine;
using System.Collections; // Thêm để dùng Coroutine

public class Shrine : MonoBehaviour
{
    private PlayerController player;
    private bool isPlayerInRange = false; // Kiểm tra xem player có trong phạm vi không
    [SerializeField] private float healInterval = 1f; // Thời gian giữa các lần hồi máu (giây)
    [SerializeField] private int healAmount = 1; // Số máu hồi mỗi lần

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                isPlayerInRange = true;
                StartCoroutine(HealOverTime()); // Bắt đầu hồi máu liên tục
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            player = null; // Xóa tham chiếu đến player
        }
    }

    private IEnumerator HealOverTime()
    {
        while (isPlayerInRange) // Tiếp tục hồi máu khi player còn trong phạm vi
        {
            if (player != null && !player.IsDie()) // Kiểm tra player còn sống
            {
                player.AddHealth(healAmount);
            }
            yield return new WaitForSeconds(healInterval); // Đợi trước khi hồi máu lần tiếp theo
        }
    }
}