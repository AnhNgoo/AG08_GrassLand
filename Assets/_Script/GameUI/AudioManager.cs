using UnityEngine;
using UnityEngine.SceneManagement;

// Quản lý âm thanh (nhạc nền và hiệu ứng âm thanh) trong game
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; } // Singleton để truy cập toàn cục

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; // Nguồn phát nhạc nền
    [SerializeField] private AudioSource sfxSource; // Nguồn phát hiệu ứng âm thanh (SFX)

    [Header("Audio Clips")]
    [SerializeField] private AudioClips audioClips; // ScriptableObject chứa các AudioClip

    private string currentScene; // Lưu tên scene hiện tại

    void Awake()
    {
        // Thiết lập singleton, giữ AudioManager qua các scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Không xóa khi chuyển scene
        }
        else
        {
            Destroy(gameObject); // Xóa nếu đã có instance
            return;
        }

        // Tạo AudioSource nếu chưa có
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true; // Nhạc nền lặp lại
        sfxSource.loop = false; // SFX không lặp

        // Lấy giá trị âm lượng từ PlayerPrefs
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
        PlayMusicForScene(SceneManager.GetActiveScene().buildIndex); // Phát nhạc theo scene
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // Đăng ký sự kiện khi scene tải
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Hủy đăng ký sự kiện
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Phát nhạc mới khi chuyển scene
        if (scene.name != currentScene)
        {
            currentScene = scene.name;
            PlayMusicForScene(scene.buildIndex); // Dùng buildIndex để chọn nhạc
        }
    }

    private void PlayMusicForScene(int sceneIndex)
    {
        AudioClip musicToPlay = null;
        // Chọn nhạc dựa trên chỉ số scene
        if (sceneIndex == 0) // MainMenu
            musicToPlay = audioClips.mainMenuMusic;
        else if (sceneIndex >= 1 && sceneIndex <= 3) // GrassLand_Map1, Map2, Map3
            musicToPlay = audioClips.gameSceneMusic;

        // Phát nhạc nếu khác nhạc hiện tại
        if (musicToPlay != null && musicSource.clip != musicToPlay)
        {
            musicSource.clip = musicToPlay;
            musicSource.Play(); // Phát nhạc nền
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        // Phát hiệu ứng âm thanh một lần
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip); // Không ghi đè SFX khác
    }

    public void PlayButtonClickSound()
    {
        PlaySFX(audioClips.buttonClickSound); // Phát âm thanh khi click nút
    }

    public void PlayAttackSFX()
    {
        PlaySFX(audioClips.attackSFX); // Phát âm thanh tấn công
    }

    public void PlayHitSFX()
    {
        PlaySFX(audioClips.hitSFX); // Phát âm thanh bị đánh
    }

    public void PlayDieSFX()
    {
        PlaySFX(audioClips.deathSFX); // Phát âm thanh khi chết
    }

    public void PlayRunSFX()
    {
        PlaySFX(audioClips.runSFX); // Phát âm thanh khi chạy
    }

    public AudioSource GetMusicSource()
    {
        return musicSource; // Trả về nguồn nhạc nền
    }

    public AudioSource GetSFXSource()
    {
        return sfxSource; // Trả về nguồn SFX
    }

    public void SetMusicVolume(float volume)
    {
        // Điều chỉnh âm lượng nhạc nền
        if (musicSource != null)
        {
            musicSource.volume = volume;
            PlayerPrefs.SetFloat("MusicVolume", volume); // Lưu vào PlayerPrefs
            PlayerPrefs.Save();
        }
    }

    public void SetSFXVolume(float volume)
    {
        // Điều chỉnh âm lượng SFX
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
            PlayerPrefs.SetFloat("SFXVolume", volume); // Lưu vào PlayerPrefs
            PlayerPrefs.Save();
        }
    }
}