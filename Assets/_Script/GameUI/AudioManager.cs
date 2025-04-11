using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClips audioClips; // ScriptableObject chứa AudioClip

    private string currentScene;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        sfxSource.loop = false;

       // Đọc giá trị từ PlayerPrefs và áp dụng
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;

    }

    void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
        PlayMusicForScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != currentScene)
        {
            currentScene = scene.name;
            PlayMusicForScene(scene.buildIndex); // Dùng buildIndex thay vì tên scene
        }
    }

    private void PlayMusicForScene(int sceneIndex)
    {
        AudioClip musicToPlay = null;
        if (sceneIndex == 0) // MainMenu (index 0)
        {
            musicToPlay = audioClips.mainMenuMusic;
        }
        else if (sceneIndex >= 1 && sceneIndex <= 3) // GrassLand_Map1, Map2, Map3 (index 1-3)
        {
            musicToPlay = audioClips.gameSceneMusic;
        }

        if (musicToPlay != null && musicSource.clip != musicToPlay)
        {
            musicSource.clip = musicToPlay;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayButtonClickSound()
    {
        PlaySFX(audioClips.buttonClickSound);
    }

    public void PlayAttackSFX()
    {
        PlaySFX(audioClips.attackSFX);
    }

    public void PlayHitSFX()
    {
        PlaySFX(audioClips.hitSFX);
    }

     public void PlayDieSFX()
    {
        PlaySFX(audioClips.deathSFX);
    }

    public void PlayRunSFX()
    {
        PlaySFX(audioClips.runSFX);
    }

    public AudioSource GetMusicSource()
    {
        return musicSource;
    }

    public AudioSource GetSFXSource()
    {
        return sfxSource;
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
            PlayerPrefs.SetFloat("MusicVolume", volume);
            PlayerPrefs.Save();
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
            PlayerPrefs.SetFloat("SFXVolume", volume);
            PlayerPrefs.Save();
        }
    }
    
}