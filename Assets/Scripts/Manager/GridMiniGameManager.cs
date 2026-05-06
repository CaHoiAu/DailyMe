using UnityEngine;

public class GridMiniGameManager : BaseMiniGameManager
{
    [SerializeField] private GridPuzzleManager puzzleManager;
    private GridMiniLevelData currentMiniLevelData;
    private bool miniGameCompleted = false;

    private void OnEnable()
    {
        // Ensure puzzle manager reference is set
        if (puzzleManager == null)
        {
            puzzleManager = GetComponent<GridPuzzleManager>();
        }
    }

    public override void LoadMiniGame(ScriptableObject minigame)
    {
        Debug.Log("Loading Grid Mini-Game...");

        GridMiniLevelData gridLevelData = minigame as GridMiniLevelData;

        if (gridLevelData == null)
        {
            Debug.LogError("Failed to load GridMiniLevelData: ScriptableObject is not of type GridMiniLevelData!");
            return;
        }

        if (puzzleManager == null)
        {
            Debug.LogError("GridPuzzleManager reference is missing! Assign it in the inspector.");
            return;
        }

        currentMiniLevelData = gridLevelData;
        miniGameCompleted = false;

        // Load the puzzle through the puzzle manager
        puzzleManager.LoadPuzzle(gridLevelData);

        Debug.Log($"Grid Mini-Game loaded: {gridLevelData.name}");
    }

    public override void ResetMiniGame()
    {
        Debug.Log("Resetting Grid Mini-Game...");

        if (puzzleManager == null || currentMiniLevelData == null)
        {
            Debug.LogError("Cannot reset: GridPuzzleManager or GridMiniLevelData is missing!");
            return;
        }

        miniGameCompleted = false;
        puzzleManager.LoadPuzzle(currentMiniLevelData);

        Debug.Log("Grid Mini-Game reset complete.");
    }

    public override bool IsMiniGameCompleted()
    {
        // Check if all objects are placed and all constraints are satisfied
        if (puzzleManager == null)
        {
            return false;
        }

        return miniGameCompleted;
    }

    /// <summary>
    /// Called by GridPuzzleManager when the puzzle is completed
    /// </summary>
    public void OnPuzzleCompleted()
    {
        miniGameCompleted = true;
        Debug.Log("Grid Mini-Game completed!");
    }
}
