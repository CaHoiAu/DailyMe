using UnityEngine;
using UnityEngine.UI;

public class TaskRow : MonoBehaviour
{
    private BaseMiniLevelData data;

    [Header("UI Elements")]
    public TMPro.TextMeshProUGUI taskNameText;
    public GameObject highlightFrame;

    public void Bind(BaseMiniLevelData miniLevelData)
    {
        data = miniLevelData;
        taskNameText.text = data.name;

        SetHighlight(false);
    }
    public void SetHighlight(bool highlight)
    {
        highlightFrame.SetActive(highlight);
    }
    public BaseMiniLevelData GetData()
    {
        return data;
    }
}
