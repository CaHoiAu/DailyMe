using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TaskTimelineManager : MonoBehaviour
{
    public List<TaskRow> taskRows = new List<TaskRow>();
    public List<BaseMiniLevelData> miniLevelDatas = new List<BaseMiniLevelData>();
    private BaseMiniGameManager miniGameManager;

    private int currentIndex = -1;

    private void Start()
    {
        for (int i = 0; i < taskRows.Count && i < miniLevelDatas.Count; i++)
        {
            taskRows[i].Bind(miniLevelDatas[i]);
        }
        MoveNext();
    }
    public void MoveNext()
    {
        if (currentIndex >= 0)
        {
            taskRows[currentIndex].SetHighlight(false);
        }
        currentIndex++;
        if (currentIndex < taskRows.Count)
        {
            taskRows[currentIndex].SetHighlight(true);
            miniGameManager.LoadMiniGame(taskRows[currentIndex].GetData());
        }
    }
}
