using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickClip;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip backgroundMusicClip;

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
        if (musicSource != null && backgroundMusicClip != null)
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
}
