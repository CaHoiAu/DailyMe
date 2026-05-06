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
        taskNameText.text = data.name;
        originalText = data.name;

        SetHighlight(false);
    }
    public void SetHighlight(bool highlight)
    {
        highlightFrame.SetActive(highlight);

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
