using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum DayState
{
    NotPlayed,
    Played,
    Current,
    Empty
}
public class DayCell : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI dayText;
    public Image background;

    public Sprite spriteCurrent;
    public Sprite spriteNotPlayed;
    public Sprite spritePlayed;

    public void SetUp(int day, DayState dayState)
    {
        dayText.text = day.ToString();

        switch(dayState)
        {
            case DayState.NotPlayed:
                background.sprite = spriteNotPlayed;
                break;
            case DayState.Played:
                background.sprite = spritePlayed;
                break;
            case DayState.Current:
                background.sprite = spriteCurrent;
                break;
            case DayState.Empty:
                background.sprite = spriteNotPlayed;
                break;
        }
    }
}
