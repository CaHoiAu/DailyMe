using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public LevelData levelData;
    //public MiniLevelManager miniLVManager;
    public GridMiniGameManager gridMNGameManager;
    public DressUpGameManager dressUpMNGameManager;
    public ConstraintManager constraintManager;
    //public DragObject[] allObjects;

    [Header("Level flow settings")]
    private bool isLevelComplete = false;

    //[Header("Optional - Multi Level")]
    //[SerializeField] private LevelData[] levels;
    //[SerializeField] private int currentLevelIndex = 0;
    private int currentMiniLVLIndex = 0;
    private BaseMiniGameManager currentManager;

    void Start()
    {
        LoadMiniLevel();
    }
    //public void OnObjectPlaced()
    //{
    //    if(!isLevelComplete)
    //        CheckLevelComplete();
    //}
    //public bool AreAllObjectsPlaced()
    //{
    //    foreach (var obj in allObjects)
    //    {
    //        if (obj.GetCurrentSlotId() == null)
    //            return false;
    //    }
    //    return true;
    //}
    //public void CheckLevelComplete()
    //{
    //    if (AreAllObjectsPlaced())
    //    {
    //        if (constraintManager.CheckAllConstraints(allObjects))
    //        {
    //            isLevelComplete = true;
    //            Debug.Log("Level Complete!");
    //        }
    //        else
    //        {
    //            Debug.Log("Level Incomplete: Constraints not satisfied.");
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("Level Incomplete: Not all objects placed.");
    //    }
    //}
    void LoadMiniLevel()
    {
        MiniGameEntry miniLevel = levelData.miniGames[currentMiniLVLIndex];
        DisableAllManagers();
        switch (miniLevel.miniLevelType)
        {
            case MiniLevelType.Arrange:
                currentManager = gridMNGameManager;
                gridMNGameManager.gameObject.SetActive(true);
                break;
            case MiniLevelType.DressUp:
                currentManager = dressUpMNGameManager;
                dressUpMNGameManager.gameObject.SetActive(true);
                break;
            default:
                Debug.LogError("Unsupported mini-game type: " + miniLevel.miniLevelType);
                return;
        }
        if (currentManager != null)
        {
            currentManager.LoadMiniGame(miniLevel.miniGameData);
        }
    }
    //void ResetLevel()
    //{
    //    Debug.Log("Resetting level...");
    //    isLevelComplete = false;
    //    //Clear toan bo slot
    //    Slot[] allSlots = FindObjectsOfType<Slot>();
    //    foreach (var slot in allSlots)
    //    {
    //        slot.ClearSlot();
    //    }
    //    //Reset toan bo object
    //    foreach (var obj in allObjects)
    //    {
    //        obj.ReturnToStartPosition();
    //    }
    //    Debug.Log("Level reset complete.");
    //}
    public void NextMiniGame()
    {
        currentMiniLVLIndex++;

        if (currentMiniLVLIndex >= levelData.miniGames.Length)
        {
            Debug.Log("Level Complete!");
            return;
        }
        LoadMiniLevel();
    }

    public void ResetCurrentMiniGame()
    {
        if (currentManager != null)
        {
            currentManager.ResetMiniGame();
        }
    }
    private void DisableAllManagers()
    {
        if(gridMNGameManager != null)
            gridMNGameManager.gameObject.SetActive(false);
        if(dressUpMNGameManager != null)
            dressUpMNGameManager.gameObject.SetActive(false);
    }
}
