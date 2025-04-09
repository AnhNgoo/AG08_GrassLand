using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Main Menu Panels")]
    [SerializeField] private GameObject menuPanel; // MenuPanel trong MainMenu
    [SerializeField] private GameObject settingPanelMainMenu; // SettingPanel trong MainMenu

    [Header("In-Game Panels")]
    [SerializeField] private GameObject pausePanel; // PausePanel trong GameScene
    [SerializeField] private GameObject settingPanelInGame; // SettingPanel trong GameScene
    [SerializeField] private Button pauseButton; // Nút Pause trong GameScene

    [Header("Audio Settings")]
    [SerializeField] private Slider musicSlider; // Slider điều chỉnh âm lượng Music
    [SerializeField] private Slider sfxSlider; // Slider điều chỉnh âm lượng SFX
    [SerializeField] private AudioSource musicSource; // AudioSource cho nhạc nền
    [SerializeField] private AudioSource sfxSource; // AudioSource cho hiệu ứng âm thanh

    private bool isPaused = false;

    void Start()
    {
        // Khởi tạo trạng thái ban đầu
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }
        if (settingPanelMainMenu != null)
        {
            settingPanelMainMenu.SetActive(false);
        }
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        if (settingPanelInGame != null)
        {
            settingPanelInGame.SetActive(false);
        }

        // Khởi tạo giá trị slider từ PlayerPrefs (nếu có)
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            if (musicSource != null)
            {
                musicSource.volume = musicSlider.value;
            }
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            if (sfxSource != null)
            {
                sfxSource.volume = sfxSlider.value;
            }
        }

        // Gán sự kiện cho slider
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    // Chức năng nút Play (MainMenu)
    public void PlayGame()
    {
        SceneManager.LoadScene("GrassLand_Map1"); // Thay "GameScene" bằng tên scene đầu tiên của bạn
    }

    // Chức năng nút Setting (MainMenu và In-Game)
    public void OpenSettingPanel()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false); // Tắt MenuPanel trong MainMenu
            settingPanelMainMenu.SetActive(true); // Mở SettingPanel trong MainMenu
        }
        else if (pausePanel != null)
        {
            pausePanel.SetActive(false); // Tắt PausePanel trong GameScene
            settingPanelInGame.SetActive(true); // Mở SettingPanel trong GameScene
        }
    }

    // Chức năng nút Back trong SettingPanel
    public void BackFromSetting()
    {
        if (settingPanelMainMenu != null && settingPanelMainMenu.activeSelf)
        {
            settingPanelMainMenu.SetActive(false); // Tắt SettingPanel trong MainMenu
            menuPanel.SetActive(true); // Mở lại MenuPanel trong MainMenu
        }
        else if (settingPanelInGame != null && settingPanelInGame.activeSelf)
        {
            settingPanelInGame.SetActive(false); // Tắt SettingPanel trong GameScene
            pausePanel.SetActive(true); // Mở lại PausePanel trong GameScene
        }
    }

    // Chức năng nút Exit (MainMenu)
    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game exited.");
    }

    // Chức năng nút Pause (In-Game)
    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            Time.timeScale = 0f; // Dừng game
            pausePanel.SetActive(true); // Mở PausePanel
            pauseButton.gameObject.SetActive(false); // Ẩn nút Pause
        }
        else
        {
            Time.timeScale = 1f; // Tiếp tục game
            pausePanel.SetActive(false); // Tắt PausePanel
            pauseButton.gameObject.SetActive(true); // Hiện lại nút Pause
        }
    }

    // Chức năng nút Continue (PausePanel)
    public void ContinueGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Tiếp tục game
        pausePanel.SetActive(false); // Tắt PausePanel
        pauseButton.gameObject.SetActive(true); // Hiện lại nút Pause
    }

    // Chức năng nút Menu (PausePanel)
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Đặt lại Time.timeScale trước khi chuyển scene
        SceneManager.LoadScene("MainMenu"); // Thay "MainMenu" bằng tên scene MainMenu của bạn
    }

    // Điều chỉnh âm lượng Music
    private void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    // Điều chỉnh âm lượng SFX
    private void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }
}