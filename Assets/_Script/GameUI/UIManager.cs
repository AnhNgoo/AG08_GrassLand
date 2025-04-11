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

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider loadingSlider;

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

        // Khởi tạo giá trị slider từ PlayerPrefs
        if (musicSlider != null)
        {
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.value = musicVolume;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            //Debug.Log($"MusicSlider initialized to: {musicVolume}");
        }
        if (sfxSlider != null)
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.value = sfxVolume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            //Debug.Log($"SFXSlider initialized to: {sfxVolume}");
        }

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    public void PlayGame()
    {
        AudioManager.Instance.PlayButtonClickSound(); // Phát âm thanh nhấn nút
        StartCoroutine(LoadSceneAsync(1));
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        // Hiển thị loading screen
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        // Load scene bất đồng bộ
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        // Đợi cho đến khi scene load xong
        while (!operation.isDone)
        {
            // Cập nhật thanh tiến trình (nếu có)
            if (loadingSlider != null)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f); // operation.progress đi từ 0 đến 0.9
                loadingSlider.value = progress;
            }

            yield return null;
        }

        // Ẩn loading screen sau khi load xong (thường không cần vì scene mới sẽ thay thế)
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
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
        SceneManager.LoadScene("MainMenu");
    }

    private void SetMusicVolume(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
    }

    private void SetSFXVolume(float volume)
    {
        AudioManager.Instance.SetSFXVolume(volume);
    }
}