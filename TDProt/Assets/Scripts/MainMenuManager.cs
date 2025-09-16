using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Pause Menu Settings")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject firstPauseButton;
    [SerializeField] private CanvasGroup pauseCanvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;

    private bool isPaused = false;
    private static Stack<string> sceneHistory = new Stack<string>();

    void Start()
    {
        InitializePauseMenu();
        RegisterCurrentScene();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    #region Scene Management
    private void RegisterCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (sceneHistory.Count == 0 || sceneHistory.Peek() != currentScene)
        {
            sceneHistory.Push(currentScene);
        }
    }

    public void LoadScene(string sceneName)
    {
        if (!DoesSceneExist(sceneName))
        {
            Debug.LogError($"Scene {sceneName} not found in build settings!");
            return;
        }

        sceneHistory.Push(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void ReturnToPreviousScene()
    {
        if (sceneHistory.Count > 1) // В стеке есть предыдущая сцена
        {
            sceneHistory.Pop(); // Удаляем текущую сцену
            string previousScene = sceneHistory.Peek();
            Time.timeScale = 1f;
            SceneManager.LoadScene(previousScene);
        }
        else
        {
            LoadMainMenu();
        }
    }

    private bool DoesSceneExist(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneNameInBuild == sceneName)
                return true;
        }
        return false;
    }
    #endregion

    #region Game Flow
    public void StartGame()
    {
        LoadScene("GameScene");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        PlayerPrefs.Save();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

    #region Menu Navigation
    public void LoadMainMenu()
    {
        sceneHistory.Clear();
        LoadScene("MainMenu");
    }

    public void LoadSettingsScene()
    {
        LoadScene("SettingsScene");
    }
    #endregion

    #region Pause System
    private void InitializePauseMenu()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
            if (pauseCanvasGroup != null)
                pauseCanvasGroup.alpha = 0;
        }
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    private void PauseGame()
    {
        if (pauseMenu == null) return;

        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        StartCoroutine(FadeIn());

        if (EventSystem.current != null && firstPauseButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstPauseButton);
        }
    }

    private void ResumeGame()
    {
        if (pauseMenu == null) return;

        StartCoroutine(FadeOut());
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
    }
    #endregion

    #region Utility
    private IEnumerator FadeIn()
    {
        pauseMenu.SetActive(true);
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            pauseCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        pauseCanvasGroup.alpha = 1;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            pauseCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        pauseCanvasGroup.alpha = 0;
        pauseMenu.SetActive(false);
    }
    #endregion
}