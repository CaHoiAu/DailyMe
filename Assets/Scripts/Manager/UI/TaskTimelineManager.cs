using System.Collections.Generic;
using UnityEngine;

public class TaskTimelineManager : MonoBehaviour
{
    [Header("Task Row Setup")]
    public GameObject taskRowPrefab;
    public GameObject taskRowPrefab2;
    public Transform taskContainer;

    private List<TaskRow> taskRows = new List<TaskRow>();
    private int currentIndex = -1;

    public void Initialize(LevelData levelData)
    {
        foreach (var row in taskRows)
            if (row != null) Destroy(row.gameObject);
        taskRows.Clear();
        currentIndex = -1;

        if (taskRowPrefab == null || taskContainer == null) return;

        for (int i = 0; i < levelData.miniGames.Length; i++)
        {
            BaseMiniLevelData data = levelData.miniGames[i].miniGameData as BaseMiniLevelData;
            if (data == null) continue;

            bool useSecond = taskRowPrefab2 != null && i % 2 == 1;
            GameObject prefab = useSecond ? taskRowPrefab2 : taskRowPrefab;

            GameObject rowObj = Instantiate(prefab, taskContainer); TaskRow row = rowObj.GetComponent<TaskRow>();
            if (row == null) continue;

            row.Bind(data);
            taskRows.Add(row);
        }
    }

    public void MoveNext()
    {
        if (currentIndex >= 0 && currentIndex < taskRows.Count)
            taskRows[currentIndex].SetHighlight(false);

        currentIndex++;

        if (currentIndex < taskRows.Count)
            taskRows[currentIndex].SetHighlight(true);
    }
}
