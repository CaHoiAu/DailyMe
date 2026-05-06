using TMPro;
using UnityEngine;

public class HeaderPlan : MonoBehaviour
{
    public TextMeshProUGUI headerText;

    private void OnEnable()
    {
        headerText.text = LevelManager.Instance.GetCurrentDayOfWeek();
    }
}
