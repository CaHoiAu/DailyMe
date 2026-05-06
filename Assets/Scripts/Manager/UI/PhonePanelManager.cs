using System.Collections.Generic;
using UnityEngine;

public class PhonePanelManager : MonoBehaviour
{
    [System.Serializable]
    public class TabPage
    {
        public string name;
        public GameObject header;
        public GameObject content;
    }
    public TabPage[] tabPages;
    public int defaultTabIndex = 0;

    private int currentTabIndex = -1;
    private Stack<int> tabHistory = new Stack<int>();

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
        if (addToHistory && currentTabIndex != -1)
        {
            tabHistory.Push(currentTabIndex);
        }
        for (int i = 0; i < tabPages.Length; i++)
        {
            bool isActive = (i == index);
            tabPages[i].header.SetActive(isActive);
            tabPages[i].content.SetActive(isActive);
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
}
