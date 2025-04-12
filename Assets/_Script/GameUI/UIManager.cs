using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Main Menu Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject settingPanelMainMenu;

    [Header("In-Game Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingPanelInGame;
    [SerializeField] private Button pauseButton;

    [Header("Audio Settings")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("UI Toggle Settings")]
    [SerializeField] private Toggle uiToggleInGame;
    [SerializeField] private GameObject mobileUICanvas;
    [SerializeField] private Button attackButton;

    private bool isPaused = false;
    private bool isMobileUI = false;

    void Start()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        if (settingPanelMainMenu != null) settingPanelMainMenu.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingPanelInGame != null) settingPanelInGame.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (mobileUICanvas != null) mobileUICanvas.SetActive(false);

        if (musicSlider != null)
        {
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.value = musicVolume;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        if (sfxSlider != null)
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.value = sfxVolume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        isMobileUI = PlayerPrefs.GetInt("IsMobileUI", 0) == 1;
        if (uiToggleInGame != null)
        {
            uiToggleInGame.isOn = isMobileUI;
            uiToggleInGame.onValueChanged.AddListener(ToggleUI);
        }
        UpdateUIState();
    }

    public void PlayGame()
    {
        AudioManager.Instance.PlayButtonClickSound();
    }

    public void OpenSettingPanel()
    {
        AudioManager.Instance.PlayButtonClickSound();
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
        Application.Quit();
        Debug.Log("Game exited.");
    }

    public void TogglePause()
    {
        AudioManager.Instance.PlayButtonClickSound();
        isPaused = !isPaused;
        if (isPaused)
        {
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
            pauseButton.gameObject.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f;
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
        pauseButton.gameObject.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        AudioManager.Instance.PlayButtonClickSound();
        Time.timeScale = 1f;
    }

    private void SetMusicVolume(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
    }

    private void SetSFXVolume(float volume)
    {
        AudioManager.Instance.SetSFXVolume(volume);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        AudioManager.Instance.PlayButtonClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ToggleUI(bool useMobileUI)
    {
        isMobileUI = useMobileUI;
        PlayerPrefs.SetInt("IsMobileUI", isMobileUI ? 1 : 0);
        PlayerPrefs.Save();
        UpdateUIState();

        if (uiToggleInGame != null) uiToggleInGame.isOn = isMobileUI;
    }

    private void UpdateUIState()
    {
        if (mobileUICanvas != null)
        {
            mobileUICanvas.SetActive(isMobileUI);
        }
    }

    public void SetAttackButtonListener(UnityEngine.Events.UnityAction attackAction)
    {
        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(attackAction);
        }
    }

    public bool IsMobileUI()
    {
        return isMobileUI;
    }
}