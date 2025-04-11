using UnityEngine;

[CreateAssetMenu(fileName = "AudioClips", menuName = "ScriptableObjects/AudioClips", order = 1)]
public class AudioClips : ScriptableObject
{
    public AudioClip mainMenuMusic;
    public AudioClip gameSceneMusic;
    public AudioClip buttonClickSound;
    public AudioClip hitSFX;
    public AudioClip deathSFX;
    public AudioClip runSFX;
    public AudioClip attackSFX;
}