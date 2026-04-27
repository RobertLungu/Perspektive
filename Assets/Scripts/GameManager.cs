using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public MenuController menuController;

    private string currentLevelScene;

    public bool IsInLevel => !string.IsNullOrEmpty(currentLevelScene);

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        if (menuController != null) menuController.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(currentLevelScene))
            SceneManager.UnloadSceneAsync(currentLevelScene);

        currentLevelScene = sceneName;
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(currentLevelScene))
            LoadLevel(currentLevelScene);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(currentLevelScene))
        {
            SceneManager.UnloadSceneAsync(currentLevelScene);
            currentLevelScene = string.Empty;
        }

        if (menuController != null)
        {
            menuController.gameObject.SetActive(true);
            menuController.OnPlay();
        }
    }
}
