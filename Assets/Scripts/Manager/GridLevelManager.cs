using UnityEngine;

public class GridLevelManager : MonoBehaviour
{
    [Header("Level Data")]
    public GridLevelData levelData;
    public GridPuzzleManager puzzleManager;

    [Header("Level flow")]
    [SerializeField] private int currentMiniLevelIndex = 0;
    [SerializeField] private bool isLevelCompleted = false;

    private void Start()
    {
        LoadMiniLevel();
    }
    private void LoadMiniLevel()
    {
        if (puzzleManager == null)
        {
            Debug.LogError("GridPuzzleManager is missing!");
            return;
        }

        if (levelData == null)
        {
            Debug.LogError("GridLevelData is missing!");
            return;
        }

        if (levelData.miniLevels == null || levelData.miniLevels.Length == 0)
        {
            Debug.LogError("Grid miniLevels array is empty!");
            return;
        }

        if (currentMiniLevelIndex < 0 || currentMiniLevelIndex >= levelData.miniLevels.Length)
        {
            Debug.LogError("Current mini level index is out of range!");
            return;
        }

        GridMiniLevelData currentMiniLevel = levelData.miniLevels[currentMiniLevelIndex];

        if (currentMiniLevel == null)
        {
            Debug.LogError("Current GridMiniLevelData is NULL!");
            return;
        }

        isLevelCompleted = false;
        puzzleManager.LoadPuzzle(currentMiniLevel);
    }
    public void OnLevelCompleted()
    {
        if (isLevelCompleted) return;

        isLevelCompleted = true;
        Debug.Log($"Mini Level {currentMiniLevelIndex + 1} completed!");
    }
    public void NextMiniLevel()
    {
        if (!isLevelCompleted)
        {
            Debug.Log("Complete the current mini level before proceeding.");
            return;
        }
        currentMiniLevelIndex++;
        if (currentMiniLevelIndex >= levelData.miniLevels.Length)
        {
            Debug.Log("All mini levels completed! Level finished!");
            // Optionally, trigger some end-of-level logic here
            return;
        }
        LoadMiniLevel();
    }
    public void ResetMiniLevel()
    {
        if (puzzleManager == null)
        {
            Debug.LogError("GridPuzzleManager is missing!");
            return;
        }
        puzzleManager.LoadPuzzle(levelData.miniLevels[currentMiniLevelIndex]);
    }
}
