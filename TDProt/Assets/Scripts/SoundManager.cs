using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickClip;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip backgroundMusicClip;

    [SerializeField] private bool musicEnabled = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayButtonClickSound()
    {
        if (audioSource != null && buttonClickClip != null)
            audioSource.PlayOneShot(buttonClickClip);
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource != null && backgroundMusicClip != null && musicEnabled)
        {
            musicSource.clip = backgroundMusicClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public bool IsMusicEnabled => musicEnabled;

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;
        if (musicSource == null) return;

        if (musicEnabled)
        {
            // Продолжаем музыку с того же места
            musicSource.UnPause();
        }
        else
        {
            // Ставим музыку на паузу
            musicSource.Pause();
        }
    }
}
