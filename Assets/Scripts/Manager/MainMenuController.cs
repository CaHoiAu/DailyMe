using Unity.VisualScripting;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private string gameSceneName = "DailyMe";

    public void OnPlayButtonClicked()
    {
        // Load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuitButtonClicked()
    {
        // Quit the application
        Application.Quit();
    }
}
