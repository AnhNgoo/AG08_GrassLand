using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    [Header("Health Bar Settings")]
    public GameObject healthBarPrefab; // Prefab của HealthBar
    public Transform worldSpaceCanvas; // Canvas chính (MainCanvas)
    public Vector3 offset = new Vector3(0, 1.2f, 0); // Vị trí offset so với character
    public float hideDelay = 3f; // Thời gian chờ trước khi ẩn thanh máu sau khi bị tổn thương

    private HealthBar healthBar;
    private PlayerController playerController;
    private EnemyAI enemyAI;
    private int maxHealth;
    private int lastHealth; // Lưu giá trị máu trước đó để kiểm tra tổn thương
    private float hideTimer; // Đếm ngược để ẩn thanh máu
    private bool isVisible = false; // Trạng thái hiển thị của thanh máu
    private Camera mainCamera;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        enemyAI = GetComponent<EnemyAI>();
        mainCamera = Camera.main;

        // Tạo thanh máu
        if (worldSpaceCanvas != null && healthBarPrefab != null)
        {
            CreateFloatingHealthBar();
        }

        // Khởi tạo giá trị máu
        if (playerController != null)
        {
            maxHealth = playerController.maxHealth;
            lastHealth = playerController.GetCurrentHealth();
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(lastHealth);
        }
        else if (enemyAI != null)
        {
            maxHealth = enemyAI.maxHealth;
            lastHealth = enemyAI.currentHealth;
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(lastHealth);
        }

        // Ẩn thanh máu ban đầu
        healthBar.Hide();
    }

    private void Update()
    {
        if (healthBar == null) return;

        // Cập nhật giá trị máu
        int currentHealth = 0;
        if (playerController != null)
        {
            currentHealth = playerController.GetCurrentHealth();
        }
        else if (enemyAI != null)
        {
            currentHealth = enemyAI.currentHealth;
        }

        // Kiểm tra nếu bị tổn thương
        if (currentHealth < lastHealth)
        {
            healthBar.Show();
            isVisible = true;
            hideTimer = hideDelay; // Reset thời gian ẩn
        }

        // Cập nhật giá trị máu
        healthBar.SetHealth(currentHealth);
        lastHealth = currentHealth;

        // Ẩn thanh máu nếu máu đầy hoặc hết thời gian chờ
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

        // Kiểm tra tầm nhìn camera (chỉ áp dụng cho enemy)
        if (enemyAI != null)
        {
            bool isInCameraView = IsInCameraView();
            if (!isInCameraView && isVisible)
            {
                healthBar.Hide();
                isVisible = false;
            }
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
        GameObject healthBarObj = Instantiate(healthBarPrefab, worldSpaceCanvas);
        healthBar = healthBarObj.GetComponent<HealthBar>();
        //Debug.Log($"Created health bar for {gameObject.name}: {healthBar != null}");
    }

    private void UpdateFloatingHealthBarPosition()
    {
        if (healthBar != null && healthBar.transform.parent == worldSpaceCanvas)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position + offset);
            healthBar.transform.position = screenPos;
            //Debug.Log($"Health bar position for {gameObject.name}: {screenPos}");
        }
    }

    private bool IsInCameraView()
    {
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1 && viewportPoint.z > 0;
    }

    private void OnDestroy()
    {
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }
    }
}