using UnityEngine;
using UnityEngine.UI;

public class MusicToggleButton : MonoBehaviour
{
    [SerializeField] private Image musicIcon;
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    private void Start()
    {
        UpdateIcon();
    }

    public void OnToggleMusic()
    {
        SoundManager.Instance.ToggleMusic();
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (SoundManager.Instance.IsMusicEnabled)
            musicIcon.sprite = musicOnSprite;
        else
            musicIcon.sprite = musicOffSprite;
    }
}
