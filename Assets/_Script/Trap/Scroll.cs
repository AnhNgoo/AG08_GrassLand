using UnityEngine;

public class Scroll : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject); // Xóa cuộn giấy khỏi ScrollList
            Debug.Log("Scroll collected!");
        }
    }
}