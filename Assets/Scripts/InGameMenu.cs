using System.Collections;
using TMPro;
using UnityEngine;

public class InGameMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausedPanel;
    public GameObject deadPanel;
    public GameObject victoryPanel;

    [Header("Death")]
    public TMP_Text deathReasonText;
    public float deathPanelDelay   = 1.1f;

    [Header("Victory")]
    public float victoryPanelDelay = 0.8f;

    public static bool CameraInputBlocked { get; private set; }

    private GridMovement player;
    private bool isDying;

    void OnEnable()
    {
        GameEvents.OnPlayerDeath += OnDeath;
        GameEvents.OnLevelWin   += OnVictory;
    }

    void OnDisable()
    {
        GameEvents.OnPlayerDeath -= OnDeath;
        GameEvents.OnLevelWin   -= OnVictory;
    }

    void Update()
    {
        if (!GameManager.Instance.IsInLevel) return;
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (pausedPanel.activeSelf)
            Resume();
        else if (!deadPanel.activeSelf && !victoryPanel.activeSelf && !isDying)
            Pause();
    }

    void Pause()
    {
        pausedPanel.SetActive(true);
        Time.timeScale = 0f;
        CameraInputBlocked = true;
        LockInput(true);
    }

    public void Resume()
    {
        pausedPanel.SetActive(false);
        Time.timeScale = 1f;
        CameraInputBlocked = false;
        LockInput(false);
    }

    void OnDeath(DeathCause cause)
    {
        isDying = true;
        LockInput(true);
        StartCoroutine(ShowPanelDelayed(deadPanel, deathPanelDelay, cause));
    }

    void OnVictory()
    {
        LockInput(true);
        CameraInputBlocked = true;
        StartCoroutine(ShowPanelDelayed(victoryPanel, victoryPanelDelay, null));
    }

    IEnumerator ShowPanelDelayed(GameObject panel, float delay, DeathCause? cause)
    {
        yield return new WaitForSeconds(delay);

        if (cause.HasValue && deathReasonText != null)
            deathReasonText.text = cause.Value switch
            {
                DeathCause.RedZone => "You entered a red zone",
                DeathCause.Crushed => "You were crushed",
                DeathCause.Bricked => "You were replaced by a brick",
                _                  => "You died"
            };

        panel.SetActive(true);
        isDying = false;
        CameraInputBlocked = true;
        LockInput(true);
    }

    public void OnRetry()
    {
        HideAll();
        GameManager.Instance.RetryLevel();
    }

    public void OnLevelSelect()
    {
        HideAll();
        GameManager.Instance.GoToMainMenu();
    }

    void HideAll()
    {
        StopAllCoroutines();
        isDying = false;
        pausedPanel.SetActive(false);
        deadPanel.SetActive(false);
        victoryPanel.SetActive(false);
        Time.timeScale = 1f;
        CameraInputBlocked = false;
        player = null;
        LockInput(false);
    }

    void LockInput(bool locked)
    {
        if (player == null) player = FindFirstObjectByType<GridMovement>();
        if (player != null) player.inputLocked = locked;
    }
}
