using System;
using System.Collections.Generic;
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
    private int[] pendingChatRestoreIndices;
    private string[] pendingObjectIds;
    private int[] pendingObjectStateIndices;
    private bool[] pendingObjectIsPlaced;
    private int[] pendingObjectGridX;
    private int[] pendingObjectGridY;
    private bool isTransitioningMiniLevel = false;

    public static LevelManager Instance { get; private set; }

    [Header("Task Timeline")]
    public TaskTimelineManager taskTimelineManager;

    [Header("Chat")]
    public ChatSequence[] chatSequences; // assign Detailed/Detailed2
    public LevelChatData[] levelChatDatas; // 1 per level

    [Header("Cutscene")]
    public CutscenePlayer cutscenePlayer;

    [Header("Loading Screen")]
    public LoadingScreenManager loadingScreenManager;

    [Header("Phone Panel")]
    public PhonePanelManager phonePanelManager;

    [Header("Header")]
    public HeaderPlan headerPlan;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
            Destroy(gameObject);
        //currentLevelIndex = PlayerPrefs.GetInt("CurrentLevelIndex", 0);
        //currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, allLevels.Length - 1);

        SaveData saveData = SaveManager.Load();
        currentLevelIndex = Mathf.Clamp(saveData.currentLevelIndex, 0, allLevels.Length - 1);
        currentMiniLVLIndex = saveData.currentMiniLVLIndex;
        pendingChatRestoreIndices = saveData.chatMessageIndices;
        pendingObjectIds = saveData.objectIds;
        pendingObjectStateIndices = saveData.objectStateIndices;
        pendingObjectIsPlaced = saveData.objectIsPlaced;
        pendingObjectGridX = saveData.objectGridX;
        pendingObjectGridY = saveData.objectGridY;
        Debug.Log($"[LevelManager][DEBUG] Awake loaded save: currentLevelIndex={saveData.currentLevelIndex}, currentMiniLevelIndex={saveData.currentMiniLVLIndex}, chatMessageIndices=[{string.Join(",", saveData.chatMessageIndices ?? new int[0])}]");
    }
    void Start()
    {
        levelData = allLevels[currentLevelIndex];
        currentMiniLVLIndex = Mathf.Clamp(currentMiniLVLIndex, 0, levelData.miniGames.Length - 1);
        taskTimelineManager?.Initialize(levelData);
        headerPlan?.Refresh();
        LoadMiniLevel();
    }
    private void PersistProgress(int[] chatIndices, string[] objectIds, int[] objectStateIndices, bool[] objectIsPlaced, int[] objectGridX, int[] objectGridY)
    {
        Debug.Log($"[LevelManager][DEBUG] PersistProgress: currentLevelIndex={currentLevelIndex}, currentMiniLevelIndex={currentMiniLVLIndex}, chatIndices=[{string.Join(",", chatIndices ?? new int[0])}]");

        SaveManager.Save(new SaveData
        {
            currentLevelIndex = currentLevelIndex,
            currentMiniLVLIndex = currentMiniLVLIndex,
            chatMessageIndices = chatIndices,
            objectIds = objectIds,
            objectStateIndices = objectStateIndices,
            objectIsPlaced = objectIsPlaced,
            objectGridX = objectGridX,
            objectGridY = objectGridY
        });
    }
    private void PersistTransition()
    {
        PersistProgress(new int[chatSequences != null ? chatSequences.Length : 0], null, null, null, null, null);
    }
    private void PersistLiveProgress()
    {
        if (isTransitioningMiniLevel) return;

        int[] chatIndices = new int[chatSequences != null ? chatSequences.Length : 0];
        for (int i = 0; i < chatIndices.Length; i++)
            chatIndices[i] = chatSequences[i] != null ? chatSequences[i].CurrentMessageIndex : 0;

        Dictionary<string, GridObjectSnapshot> objectStates = currentManager?.GetObjectStateSnapshot();
        string[] objectIds = null;
        int[] objectStateIndices = null;
        bool[] objectIsPlaced = null;
        int[] objectGridX = null;
        int[] objectGridY = null;
        if (objectStates != null)
        {
            objectIds = new string[objectStates.Count];
            objectStateIndices = new int[objectStates.Count];
            objectIsPlaced = new bool[objectStates.Count];
            objectGridX = new int[objectStates.Count];
            objectGridY = new int[objectStates.Count];
            int idx = 0;
            foreach (var kvp in objectStates)
            {
                objectIds[idx] = kvp.Key;
                objectStateIndices[idx] = kvp.Value.stateIndex;
                objectIsPlaced[idx] = kvp.Value.isPlaced;
                objectGridX[idx] = kvp.Value.gridPosition.x;
                objectGridY[idx] = kvp.Value.gridPosition.y;
                idx++;
            }
        }

        PersistProgress(chatIndices, objectIds, objectStateIndices, objectIsPlaced, objectGridX, objectGridY);
    }
    public void PersistChatProgress()
    {
        PersistLiveProgress();
    }

    public void PersistObjectStateProgress()
    {
        PersistLiveProgress();
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
        Debug.Log($"[LevelManager][DEBUG] LoadMiniLevelContent: currentMiniLevelIndex={currentMiniLVLIndex}, miniLevelType={miniLevel.miniLevelType}");

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
            if (pendingObjectIds != null && pendingObjectStateIndices != null)
            {
                var snapshot = new Dictionary<string, GridObjectSnapshot>();
                for (int i = 0; i < pendingObjectIds.Length && i < pendingObjectStateIndices.Length; i++)
                {
                    snapshot[pendingObjectIds[i]] = new GridObjectSnapshot
                    {
                        stateIndex = pendingObjectStateIndices[i],
                        isPlaced = pendingObjectIsPlaced != null && i < pendingObjectIsPlaced.Length && pendingObjectIsPlaced[i],
                        gridPosition = new Vector2Int(
                            pendingObjectGridX != null && i < pendingObjectGridX.Length ? pendingObjectGridX[i] : 0,
                            pendingObjectGridY != null && i < pendingObjectGridY.Length ? pendingObjectGridY[i] : 0)
                    };
                }
                currentManager.ApplyObjectStateSnapshot(snapshot);
            }
        }
        pendingObjectIds = null;
        pendingObjectStateIndices = null;
        pendingObjectIsPlaced = null;
        pendingObjectGridX = null;
        pendingObjectGridY = null;
        taskTimelineManager?.MoveNext(currentMiniLVLIndex);
        // ✅ Load chat sequences cho mini level này
        LoadChatSequences(miniLevel.contactSequences);
       
        FindObjectOfType<VerifyButtonController>()?.ResetAll();

        // ✅ Quay về danh sách contact, tránh để tab detail mess cũ đè lên khi mở lại tab Messages
        FindObjectOfType<ChatManager>()?.ShowContactList();

        foreach (var seq in chatSequences)
            seq.ResumeFromLevelBreak();

        if (pendingChatRestoreIndices != null)
        {
            for (int i = 0; i < chatSequences.Length && i < pendingChatRestoreIndices.Length; i++)
                chatSequences[i].RestoreMessageIndex(pendingChatRestoreIndices[i]);
            pendingChatRestoreIndices = null;
        }
        isTransitioningMiniLevel = false;
        // ✅ Mini level chỉ có cutscene → tự động chuyển tiếp, không có gameplay
        if (miniLevel.miniLevelType == MiniLevelType.CutsceneOnly)
            NextMiniGame();
    }
    private void LoadChatSequences(ChatSequenceData[] sequenceDatas)
    {
        if (chatSequences == null) return;

        for (int i = 0; i < chatSequences.Length; i++)
        {
            if (sequenceDatas != null && i < sequenceDatas.Length && sequenceDatas[i] != null)
            {
                chatSequences[i].LoadData(sequenceDatas[i]);
                Debug.Log($"[LevelManager] Loaded chat sequence {i}");
            }
            else
            {
                // Không có data → reset chat trống
                chatSequences[i].ClearChat();
            }
        }
    }
    public void NextMiniGame()
    {
        Debug.Log($"[LevelManager][DEBUG] NextMiniGame called, instanceId={GetInstanceID()}, currentMiniLevelIndex {currentMiniLVLIndex} -> {currentMiniLVLIndex + 1}");

        isTransitioningMiniLevel = true;
        currentMiniLVLIndex++;
        phonePanelManager?.ResetToDefaultTab();

        if (currentMiniLVLIndex >= levelData.miniGames.Length)
        {
            Debug.Log("Level Complete!");
            CompleteCurrentLevel();
            return;
        }
        PersistTransition();
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
            currentMiniLVLIndex = 0;
            levelData = allLevels[currentLevelIndex];
            taskTimelineManager?.Initialize(levelData);
            headerPlan?.Refresh();
            PersistTransition();
            LoadMiniLevel();
            if (loadingScreenManager != null)
                loadingScreenManager.Show(LoadMiniLevel);
            else
                LoadMiniLevel();
        }
        else
        {
            Debug.Log("All levels completed! No more levels to load.");
        }
    }
}
