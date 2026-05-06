using UnityEditor.Animations;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    public GameObject dayCellPrefab;
    public Transform gridContainer;

    private void OnEnable()
    {
        GenerateCalendar();
    }

    void GenerateCalendar()
    {
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
        int daysInMonth = System.DateTime.DaysInMonth(System.DateTime.Now.Year, System.DateTime.Now.Month);
        for (int day = 1; day <= daysInMonth; day++)
        {
            DayState dayState = LevelManager.Instance.GetDayState(day);
            GameObject dayCellObj = Instantiate(dayCellPrefab, gridContainer);
            dayCellObj.GetComponent<DayCell>().SetUp(day, dayState);
        }
    }
}
