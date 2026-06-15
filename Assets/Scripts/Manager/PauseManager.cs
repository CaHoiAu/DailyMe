using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject pausePanel;

    public bool isPaused { get; private set; }

    void Awake()
    {
        Instance = this;
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Time.timeScale = 1f;
    }
}