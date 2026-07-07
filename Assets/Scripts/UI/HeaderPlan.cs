using TMPro;
using UnityEngine;

public class HeaderPlan : MonoBehaviour
{
    public TextMeshProUGUI headerText;

    private void OnEnable()
    {
        Refresh();
    }

    // Gọi từ LevelManager mỗi khi currentLevelIndex thay đổi (vd. qua ngày mới), tránh
    // header chỉ được cập nhật lúc OnEnable — bị đứng giá trị cũ nếu tab này không bị tắt/mở lại.
    public void Refresh()
    {
        headerText.text = LevelManager.Instance.GetCurrentDayOfWeek();
    }
}
