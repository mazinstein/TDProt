using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickClip;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip backgroundMusicClip;

    [SerializeField] private bool musicEnabled = true;

    [SerializeField] private AudioClip towerShootClip1;
    [SerializeField] private AudioClip towerShootClip2;
    [SerializeField] private AudioClip towerShootClip3;
    [SerializeField] private AudioClip defeatClip;

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
            // Ïðîäîëæàåì ìóçûêó ñ òîãî æå ìåñòà
            musicSource.UnPause();
        }
        else
        {
            // Ñòàâèì ìóçûêó íà ïàóçó
            musicSource.Pause();
        }
    }

    public void PlayTowerShootSound(int towerIndex)
    {
        if (audioSource == null) return;
        AudioClip clip = null;
        switch (towerIndex)
        {
            case 1: clip = towerShootClip1; break;
            case 2: clip = towerShootClip2; break;
            case 3: clip = towerShootClip3; break;
        }
        if (clip != null)
            audioSource.PlayOneShot(clip, 0.3f); // 0.3f — громкость эффекта
    }

    public void PlayDefeatSound()
    {
        if (audioSource != null && defeatClip != null)
            audioSource.PlayOneShot(defeatClip, 1f); // 0.7f — громкость, можно изменить
    }
}


