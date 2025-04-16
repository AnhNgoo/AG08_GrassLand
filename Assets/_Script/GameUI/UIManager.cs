using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Quản lý giao diện người dùng (UI) trong game
public class UIManager : MonoBehaviour
{
    [Header("Main Menu Panels")]
    [SerializeField] private GameObject menuPanel; // Panel menu chính
    [SerializeField] private GameObject settingPanelMainMenu; // Panel cài đặt ở menu

    [Header("In-Game Panels")]
    [SerializeField] private GameObject pausePanel; // Panel tạm dừng
    [SerializeField] private GameObject settingPanelInGame; // Panel cài đặt trong game
    [SerializeField] private Button pauseButton; // Nút tạm dừng

    [Header("Audio Settings")]
    [SerializeField] private Slider musicSlider; // Thanh trượt âm lượng nhạc
    [SerializeField] private Slider sfxSlider; // Thanh trượt âm lượng hiệu ứng

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel; // Panel game over

    [Header("UI Toggle Settings")]
    [SerializeField] private Toggle uiToggleInGame; // Toggle chuyển UI mobile
    [SerializeField] private GameObject mobileUICanvas; // Canvas UI mobile
    [SerializeField] private Button attackButton; // Nút tấn công cho mobile

    private bool isPaused = false; // Trạng thái tạm dừng
    private bool isMobileUI = false; // Trạng thái UI mobile

    void Start()
    {
        // Khởi tạo trạng thái các panel
        if (menuPanel != null) menuPanel.SetActive(true);
        if (settingPanelMainMenu != null) settingPanelMainMenu.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingPanelInGame != null) settingPanelInGame.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (mobileUICanvas != null) mobileUICanvas.SetActive(false);

        // Thiết lập thanh trượt âm lượng
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.onValueChanged.AddListener(SetMusicVolume); // Gán sự kiện thay đổi âm lượng
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        // Thiết lập toggle UI mobile
        isMobileUI = PlayerPrefs.GetInt("IsMobileUI", 0) == 1;
        if (uiToggleInGame != null)
        {
            uiToggleInGame.isOn = isMobileUI;
            uiToggleInGame.onValueChanged.AddListener(ToggleUI); // Gán sự kiện toggle
        }
        UpdateUIState();
    }

    public void PlayGame()
    {
        AudioManager.Instance.PlayButtonClickSound(); // Phát âm thanh click
    }

    public void OpenSettingPanel()
    {
        AudioManager.Instance.PlayButtonClickSound();
        // Chuyển từ menu/pause sang cài đặt
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            settingPanelMainMenu.SetActive(true);
        }
        else if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            settingPanelInGame.SetActive(true);
        }
    }

    public void BackFromSetting()
    {
        AudioManager.Instance.PlayButtonClickSound();
        // Quay lại từ cài đặt
        if (settingPanelMainMenu != null && settingPanelMainMenu.activeSelf)
        {
            settingPanelMainMenu.SetActive(false);
            menuPanel.SetActive(true);
        }
        else if (settingPanelInGame != null && settingPanelInGame.activeSelf)
        {
            settingPanelInGame.SetActive(false);
            pausePanel.SetActive(true);
        }
    }

    public void ExitGame()
    {
        AudioManager.Instance.PlayButtonClickSound();
        Application.Quit(); // Thoát game
    }

    public void TogglePause()
    {
        AudioManager.Instance.PlayButtonClickSound();
        isPaused = !isPaused;
        // Tạm dừng hoặc tiếp tục game
        if (isPaused)
        {
            Time.timeScale = 0f; // Dừng thời gian
            pausePanel.SetActive(true);
            pauseButton.gameObject.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f; // Tiếp tục thời gian
            pausePanel.SetActive(false);
            pauseButton.gameObject.SetActive(true);
        }
    }

    public void ContinueGame()
    {
        AudioManager.Instance.PlayButtonClickSound();
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        pauseButton.gameObject.SetActive(true); // Tiếp tục game
    }

    public void ReturnToMainMenu()
    {
        AudioManager.Instance.PlayButtonClickSound();
        Time.timeScale = 1f; // Đặt lại thời gian
    }

    private void SetMusicVolume(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume); // Điều chỉnh âm lượng nhạc
    }

    private void SetSFXVolume(float volume)
    {
        AudioManager.Instance.SetSFXVolume(volume); // Điều chỉnh âm lượng hiệu ứng
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true); // Hiển thị panel game over
        Time.timeScale = 0f; // Dừng game
    }

    public void RestartGame()
    {
        AudioManager.Instance.PlayButtonClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Tải lại scene hiện tại
    }

    private void ToggleUI(bool useMobileUI)
    {
        isMobileUI = useMobileUI;
        PlayerPrefs.SetInt("IsMobileUI", isMobileUI ? 1 : 0); // Lưu trạng thái UI
        PlayerPrefs.Save();
        UpdateUIState();
        if (uiToggleInGame != null) uiToggleInGame.isOn = isMobileUI;
    }

    private void UpdateUIState()
    {
        if (mobileUICanvas != null)
            mobileUICanvas.SetActive(isMobileUI); // Bật/tắt UI mobile
    }

    public void SetAttackButtonListener(UnityEngine.Events.UnityAction attackAction)
    {
        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(attackAction); // Gán sự kiện cho nút tấn công
        }
    }

    public bool IsMobileUI()
    {
        return isMobileUI; // Trả về trạng thái UI mobile
    }
}