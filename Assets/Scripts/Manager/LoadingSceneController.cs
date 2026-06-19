using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private string targetScene = "DailyMe";

    private void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;
        while (!operation.isDone)
        {
            // Cập nhật thanh tiến trình (operation.progress từ 0 đến 0.9)
            progressBar.value = operation.progress / 0.9f;
            // Khi load xong (operation.progress >= 0.9), cho phép chuyển cảnh
            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
