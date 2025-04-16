using UnityEngine;

// Quản lý thanh máu nổi trên nhân vật hoặc enemy
public class HealthBarManager : MonoBehaviour
{
    [Header("Health Bar Settings")]
    public GameObject healthBarPrefab; // Prefab của HealthBar
    public Transform worldSpaceCanvas; // Canvas chứa thanh máu
    public Vector3 offset = new Vector3(0, 1.2f, 0); // Vị trí lệch so với nhân vật
    public float hideDelay = 3f; // Thời gian chờ trước khi ẩn thanh máu

    private HealthBar healthBar; // Thanh máu
    private PlayerController playerController; // Tham chiếu đến PlayerController
    private EnemyAI enemyAI; // Tham chiếu đến EnemyAI
    private int maxHealth; // Máu tối đa
    private int lastHealth; // Máu trước đó để kiểm tra thay đổi
    private float hideTimer; // Đếm ngược để ẩn thanh máu
    private bool isVisible = false; // Trạng thái hiển thị
    private Camera mainCamera; // Camera chính

    private void Start()
    {
        // Khởi tạo thành phần
        playerController = GetComponent<PlayerController>();
        enemyAI = GetComponent<EnemyAI>();
        mainCamera = Camera.main;

        // Tạo thanh máu
        if (worldSpaceCanvas != null && healthBarPrefab != null)
            CreateFloatingHealthBar();

        // Thiết lập máu ban đầu
        if (playerController != null)
        {
            maxHealth = playerController.maxHealth;
            lastHealth = playerController.GetCurrentHealth();
        }
        else if (enemyAI != null)
        {
            maxHealth = enemyAI.maxHealth;
            lastHealth = enemyAI.currentHealth;
        }
        healthBar.SetMaxHealth(maxHealth); // Đặt máu tối đa
        healthBar.SetHealth(lastHealth); // Đặt máu hiện tại
        healthBar.Hide(); // Ẩn ban đầu
    }

    private void Update()
    {
        if (healthBar == null) return;

        // Lấy máu hiện tại
        int currentHealth = playerController != null ? playerController.GetCurrentHealth() : enemyAI.currentHealth;

        // Hiển thị thanh máu nếu máu thay đổi
        if (currentHealth < lastHealth || currentHealth > lastHealth)
        {
            healthBar.Show();
            isVisible = true;
            hideTimer = hideDelay; // Reset thời gian ẩn
        }

        // Cập nhật giá trị máu
        healthBar.SetHealth(currentHealth); // Cập nhật thanh máu
        lastHealth = currentHealth;

        // Ẩn thanh máu nếu máu đầy/hết hoặc hết thời gian
        if (currentHealth >= maxHealth || currentHealth <= 0)
        {
            healthBar.Hide();
            isVisible = false;
        }
        else if (isVisible)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
            {
                healthBar.Hide();
                isVisible = false;
            }
        }

        // Kiểm tra tầm nhìn camera cho enemy
        if (enemyAI != null)
        {
            bool isInCameraView = IsInCameraView();
            if (!isInCameraView && isVisible)
                healthBar.Hide();
            else if (isInCameraView && !isVisible && currentHealth < maxHealth && currentHealth > 0)
            {
                healthBar.Show();
                isVisible = true;
            }
        }

        // Cập nhật vị trí thanh máu
        UpdateFloatingHealthBarPosition();
    }

    private void CreateFloatingHealthBar()
    {
        // Tạo thanh máu từ prefab
        GameObject healthBarObj = Instantiate(healthBarPrefab, worldSpaceCanvas);
        healthBar = healthBarObj.GetComponent<HealthBar>(); // Lấy component HealthBar
    }

    private void UpdateFloatingHealthBarPosition()
    {
        // Cập nhật vị trí thanh máu theo nhân vật
        if (healthBar != null && healthBar.transform.parent == worldSpaceCanvas)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position + offset);
            healthBar.transform.position = screenPos; // Đặt vị trí trên màn hình
        }
    }

    private bool IsInCameraView()
    {
        // Kiểm tra nhân vật có trong tầm nhìn camera không
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1 && viewportPoint.z > 0;
    }

    private void OnDestroy()
    {
        // Xóa thanh máu khi nhân vật bị hủy
        if (healthBar != null)
            Destroy(healthBar.gameObject);
    }
}