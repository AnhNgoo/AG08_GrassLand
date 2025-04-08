using UnityEngine;
using UnityEngine.UI; // Thêm dòng này để sử dụng Slider và Image
using TMPro;

// Script này sẽ được gắn vào player hoặc enemy để quản lý thanh máu của họ
public class HealthBarManager : MonoBehaviour
{
    [Header("Health Bar Settings")]
    public HealthBar healthBar;
    public Transform worldSpaceCanvas; // Canvas cho thanh máu floating
    public Vector3 offset = new Vector3(0, 1.2f, 0); // Vị trí offset so với character
    
    // Reference đến component quản lý máu (Player hoặc Enemy)
    private PlayerController playerController;
    private EnemyAI enemyAI;
    
    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        enemyAI = GetComponent<EnemyAI>();
        
        // Nếu là player
        if (playerController != null)
        {
            // Nếu thanh máu không phải là floating, đã được set trong inspector
            if (healthBar != null)
            {
                healthBar.SetMaxHealth(playerController.maxHealth);
            }
        }
        // Nếu là enemy
        else if (enemyAI != null)
        {
            // Tạo thanh máu floating nếu canvas đã được set
            if (worldSpaceCanvas != null && healthBar == null)
            {
                CreateFloatingHealthBar();
            }
            
            if (healthBar != null)
            {
                healthBar.SetMaxHealth(enemyAI.maxHealth);
            }
        }
    }
    
    private void Update()
    {
        // Cập nhật giá trị máu hiện tại
        if (healthBar != null)
        {
            if (playerController != null)
            {
                healthBar.SetHealth(playerController.GetCurrentHealth());
            }
            else if (enemyAI != null)
            {
                healthBar.SetHealth(enemyAI.currentHealth);
                
                // Cập nhật vị trí của thanh máu floating nếu enemy
                UpdateFloatingHealthBarPosition();
            }
        }
    }
    
    private void CreateFloatingHealthBar()
    {
        // Tạo một game object mới cho thanh máu
        GameObject healthBarObj = new GameObject("EnemyHealthBar");
        healthBarObj.transform.SetParent(worldSpaceCanvas);
        
        // Thêm các component cần thiết
        RectTransform rectTransform = healthBarObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(80, 10); // Kích thước thanh máu
        
        // Thêm Slider component
        Slider slider = healthBarObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = enemyAI.maxHealth;
        slider.value = enemyAI.currentHealth;
        slider.interactable = false;
        
        // Tạo background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(healthBarObj.transform);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.sizeDelta = Vector2.zero;
        
        // Tạo fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(healthBarObj.transform);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0);
        fillAreaRect.anchorMax = new Vector2(1, 1);
        fillAreaRect.offsetMin = new Vector2(2, 2);
        fillAreaRect.offsetMax = new Vector2(-2, -2);
        
        // Tạo fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.red;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.sizeDelta = Vector2.zero;
        
        // Setup slider
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        slider.direction = Slider.Direction.LeftToRight;
        
        // Tạo HealthBar component và liên kết với slider
        healthBar = healthBarObj.AddComponent<HealthBar>();
        healthBar.slider = slider;
        healthBar.fill = fillImage;
        
        // Thiết lập gradient màu
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.red, 0.0f), 
                new GradientColorKey(Color.yellow, 0.5f), 
                new GradientColorKey(Color.green, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(1.0f, 1.0f) 
            }
        );
        healthBar.gradient = gradient;
        
        // Cập nhật vị trí ban đầu
        UpdateFloatingHealthBarPosition();
    }
    
    private void UpdateFloatingHealthBarPosition()
    {
        if (healthBar != null && healthBar.transform.parent == worldSpaceCanvas)
        {
            // Chuyển đổi vị trí từ world space sang canvas space
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + offset);
            healthBar.transform.position = screenPos;
            
            // Ẩn thanh máu nếu enemy chết
            if (enemyAI != null && enemyAI.currentHealth <= 0)
            {
                healthBar.gameObject.SetActive(false);
            }
        }
    }
}