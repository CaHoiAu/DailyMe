using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public LevelData[] allLevels;
    public LevelData levelData { get; private set; }
    public GridMiniGameManager gridMNGameManager;
    public DressUpGameManager dressUpMNGameManager;
    public ConstraintManager constraintManager;

    [Header("Level flow settings")]
    private bool isLevelComplete = false;
    private int currentMiniLVLIndex = 0;
    private BaseMiniGameManager currentManager;

    private int currentLevelIndex = 0;
    public static LevelManager Instance { get; private set; }

    [Header("Task Timeline")]
    public TaskTimelineManager taskTimelineManager;

    [Header("Chat")]
    public ChatSequence[] chatSequences; // assign Detailed/Detailed2
    public LevelChatData[] levelChatDatas; // 1 per level

    [Header("Cutscene")]
    public CutscenePlayer cutscenePlayer;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else
            Destroy(gameObject);
        currentLevelIndex = PlayerPrefs.GetInt("CurrentLevelIndex", 0);
        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, allLevels.Length - 1);
    }
    void Start()
    {
        levelData = allLevels[currentLevelIndex];
        taskTimelineManager?.Initialize(levelData);
        LoadMiniLevel();
    }
    void LoadMiniLevel()
    {
        MiniGameEntry miniLevel = levelData.miniGames[currentMiniLVLIndex];

        if (cutscenePlayer != null)
        {
            cutscenePlayer.Play(miniLevel.cutscene, () => LoadMiniLevelContent(miniLevel));
        }
        else
        {
            LoadMiniLevelContent(miniLevel);
        }
    }

    private void LoadMiniLevelContent(MiniGameEntry miniLevel)
    {
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
            case MiniLevelType.CutsceneOnly:
                currentManager = null;
                break;
            default:
                Debug.LogError("Unsupported mini-game type: " + miniLevel.miniLevelType);
                return;
        }
        if (currentManager != null)
        {
            currentManager.LoadMiniGame(miniLevel.miniGameData);
        }
        taskTimelineManager?.MoveNext();
        // ✅ Load chat sequences cho mini level này
        LoadChatSequences(miniLevel.contactSequences);

        FindObjectOfType<VerifyButtonController>()?.ResetAll();
        foreach (var seq in chatSequences)
            seq.ResumeFromLevelBreak();
        // ✅ Mini level chỉ có cutscene → tự động chuyển tiếp, không có gameplay
        if (miniLevel.miniLevelType == MiniLevelType.CutsceneOnly)
            NextMiniGame();
    }
    private void LoadChatSequences(ChatSequenceData[] sequenceDatas)
    {
        if (sequenceDatas == null || chatSequences == null) return;

        for (int i = 0; i < chatSequences.Length; i++)
        {
            if (i < sequenceDatas.Length && sequenceDatas[i] != null)
            {
                chatSequences[i].LoadData(sequenceDatas[i]);
                Debug.Log($"[LevelManager] Loaded chat sequence {i}");
            }
            else
            {
                // Không có data → reset chat trống
                chatSequences[i].ResetChat();
            }
        }
    }
    public void NextMiniGame()
    {
        currentMiniLVLIndex++;

        if (currentMiniLVLIndex >= levelData.miniGames.Length)
        {
            Debug.Log("Level Complete!");
            CompleteCurrentLevel();
            return;
        }
        LoadMiniLevel();
    }
    public void OnVerifyClicked()
    {
        currentManager?.OnVerifyButtonClicked();
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
    public DayState GetDayState(int day)
    {
        int dayIndex = GetLevelIndexForDay(day);
        if (dayIndex == -1)
            return DayState.Empty; // No level for this day
        if (dayIndex < currentLevelIndex)
            return DayState.Played;
        if (dayIndex == currentLevelIndex)
            return DayState.Current;
        if (dayIndex > currentLevelIndex)
            return DayState.NotPlayed;
        return DayState.NotPlayed; // Default case (should not reach here)
    }

    private int GetLevelIndexForDay(int day)
    {
        for (int i = 0; i < allLevels.Length; i++)
        {
            if (allLevels[i].dayNumber == day)
                return i;
        }
        return -1; // Not found
    }
    public string GetCurrentDayOfWeek()
    {
        return DayOfWeekToVietnamese(allLevels[currentLevelIndex].dayOfWeek);
    }
    private string DayOfWeekToVietnamese(System.DayOfWeek dayOfWeek)
    {
        switch (dayOfWeek)
        {
            case System.DayOfWeek.Monday: return "THỨ 2";
            case System.DayOfWeek.Tuesday: return "THỨ 3";
            case System.DayOfWeek.Wednesday: return "THỨ 4";
            case System.DayOfWeek.Thursday: return "THỨ 5";
            case System.DayOfWeek.Friday: return "THỨ 6";
            case System.DayOfWeek.Saturday: return "THỨ 7";
            case System.DayOfWeek.Sunday: return "CHỦ NHẬT";
            default: return "";
        }
    }

    //Khi nguoi choi hoan thanh toan bo level (ngay)
    public void CompleteCurrentLevel()
    {
        if (currentLevelIndex < allLevels.Length - 1)
        {
            currentLevelIndex++;
            PlayerPrefs.SetInt("CurrentLevelIndex", currentLevelIndex);
            PlayerPrefs.Save();
            levelData = allLevels[currentLevelIndex];
            currentMiniLVLIndex = 0;
        }
        else
        {
            Debug.Log("All levels completed! No more levels to load.");
        }
    }
}
