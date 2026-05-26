using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhonePanelManager : MonoBehaviour
{
    [System.Serializable]
    public class TabPage
    {
        public string name;
        public GameObject header;
        public GameObject content;
        public Button tabButton; // ✅ Thêm button reference
    }
    public TabPage[] tabPages;
    public int defaultTabIndex = 0;

    private int currentTabIndex = -1;
    private Stack<int> tabHistory = new Stack<int>();
    private Button currentActiveButton = null; // ✅ Lưu button đang active
    private bool isTabsLocked = false; // ✅ Kiểm tra có đang lock tabs không

    public void Start()
    {
        ShowTabPage(defaultTabIndex, false);
    }

    public void ShowTabUI(int index)
    {
        ShowTabPage(index, true);
    }

    private void ShowTabPage(int index, bool addToHistory = true)
    {
        if (index < 0 || index >= tabPages.Length)
        {
            Debug.LogError("Invalid tab index: " + index);
            return;
        }

        // ❌ Nếu tabs bị lock, không cho chuyển tab (ngoài trừ GoBack)
        if (isTabsLocked && addToHistory)
        {
            Debug.Log("⛔ Tabs are locked! Cannot switch during chat sequence.");
            return;
        }

        if (addToHistory && currentTabIndex != -1)
        {
            tabHistory.Push(currentTabIndex);
        }

        // ✅ Enable button cũ trước khi switch
        if (currentActiveButton != null)
        {
            currentActiveButton.interactable = true;
        }

        for (int i = 0; i < tabPages.Length; i++)
        {
            bool isActive = (i == index);
            tabPages[i].header.SetActive(isActive);
            tabPages[i].content.SetActive(isActive);

            // ✅ Disable button của tab được click
            if (isActive && tabPages[i].tabButton != null)
            {
                tabPages[i].tabButton.interactable = false;
                currentActiveButton = tabPages[i].tabButton;
                Debug.Log($"✅ Locked button for tab: {tabPages[i].name}");
            }
        }

        currentTabIndex = index;
    }

    public void GoBack()
    {
        if (tabHistory.Count > 0)
        {
            int previousTabIndex = tabHistory.Pop();
            ShowTabPage(previousTabIndex, false);
        }
    }

    // ✅ Khóa tất cả tab buttons (khi vào chat sequence)
    public void LockAllTabs()
    {
        isTabsLocked = true;
        foreach (var tab in tabPages)
        {
            if (tab.tabButton != null)
                tab.tabButton.interactable = false;
        }
        Debug.Log("🔒 All tabs are now LOCKED");
    }

    // ✅ Mở khóa tất cả tab buttons (khi thoát chat sequence)
    public void UnlockAllTabs()
    {
        isTabsLocked = false;
        foreach (var tab in tabPages)
        {
            if (tab.tabButton != null)
                tab.tabButton.interactable = true;
        }
        Debug.Log("🔓 All tabs are now UNLOCKED");
    }

    // ✅ Kiểm tra xem tabs có bị lock không
    public bool AreTabsLocked()
    {
        return isTabsLocked;
    }
}
