using UnityEngine;
using UnityEngine.UIElements;

public class GameHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument uiDocument; // UI Document cho HUD
    [SerializeField] private GameObject enemyList; // Danh sách enemy
    [SerializeField] private GameObject scrollList; // Danh sách scroll

    private Label enemiesKilledLabel; // Label hiển thị số enemy đã giết
    private Label scrollsCollectedLabel; // Label hiển thị số scroll đã nhặt
    private int initialEnemyCount; // Số enemy ban đầu
    private int initialScrollCount; // Số scroll ban đầu

    void Start()
    {
        // Lưu số lượng ban đầu của enemy và scroll
        initialEnemyCount = enemyList != null ? enemyList.transform.childCount : 0;
        initialScrollCount = scrollList != null ? scrollList.transform.childCount : 0;

        // Khởi tạo UI Toolkit
        if (uiDocument != null)
        {
            VisualElement root = uiDocument.rootVisualElement;
            enemiesKilledLabel = root.Q<Label>("EnemiesKilledLabel");
            scrollsCollectedLabel = root.Q<Label>("ScrollsCollectedLabel");
            UpdateHUD(); // Cập nhật UI ban đầu
        }
        else
        {
            Debug.LogWarning("UI Document is not assigned in GameHUD!");
        }
    }

    void Update()
    {
        UpdateHUD(); // Cập nhật UI mỗi frame
    }

    void UpdateHUD()
    {
        if (enemiesKilledLabel != null && enemyList != null)
        {
            int currentEnemyCount = enemyList.transform.childCount;
            int enemiesKilled = initialEnemyCount - currentEnemyCount;
            enemiesKilledLabel.text = $"Enemies Killed: {enemiesKilled}";
        }

        if (scrollsCollectedLabel != null && scrollList != null)
        {
            int currentScrollCount = scrollList.transform.childCount;
            int scrollsCollected = initialScrollCount - currentScrollCount;
            scrollsCollectedLabel.text = $"Scrolls Collected: {scrollsCollected}";
        }
    }
}