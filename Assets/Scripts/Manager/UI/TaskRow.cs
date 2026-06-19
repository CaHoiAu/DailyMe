using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskRow : MonoBehaviour
{
    private BaseMiniLevelData data;

    [Header("UI Elements")]
    public TMPro.TextMeshProUGUI taskNameText;
    public GameObject highlightFrame;
    public GameObject background;
    private string originalText;

    //style bthg
    public Color normalColor;

    //style highlight
    public Color highlightColor;
    public void Bind(BaseMiniLevelData miniLevelData)
    {
        data = miniLevelData;
        string displayName = !string.IsNullOrEmpty(data.name)
            ? data.name
            : ((UnityEngine.Object)data).name;
        taskNameText.text = displayName;
        originalText = displayName;

        SetHighlight(false);
    }
    public void SetHighlight(bool highlight)
    {
        highlightFrame.SetActive(highlight);
        Debug.Log($"[TaskRow][DEBUG] {name} SetHighlight({highlight}) -> highlightFrame.activeInHierarchy={highlightFrame.activeInHierarchy}, row.activeInHierarchy={gameObject.activeInHierarchy}");
        if (string.IsNullOrEmpty(originalText))
            originalText = taskNameText.text;
        if (highlight)
        {
            taskNameText.color = highlightColor;
            taskNameText.text = originalText.ToUpper();
            background.SetActive(true);
        }
        else
        {
            taskNameText.color = normalColor;
            taskNameText.text = originalText;
            background.SetActive(false);
        }
    }
    public BaseMiniLevelData GetData()
    {
        return data;
    }
}
