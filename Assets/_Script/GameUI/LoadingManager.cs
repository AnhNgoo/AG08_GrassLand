using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public GameObject loadingScreen;
    public Slider progressBar;
    public TextMeshProUGUI progressText; 

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f; // Đặt lại thời gian về bình thường trước khi chuyển cảnh
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float displayProgress = 0f;

        while (operation.progress < 0.9f)
        {
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            displayProgress = Mathf.MoveTowards(displayProgress, realProgress, Time.deltaTime * 0.5f);
            progressBar.value = displayProgress;

            if (progressText != null)
                progressText.text = "Loading " + (displayProgress * 100f).ToString("F0") + "%";

            yield return null;
        }

        while (displayProgress < 1f)
        {
            displayProgress = Mathf.MoveTowards(displayProgress, 1f, Time.deltaTime * 0.5f);
            progressBar.value = displayProgress;

            if (progressText != null)
                progressText.text = "Loading " + (displayProgress * 100f).ToString("F0") + "%";

            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // delay nhẹ cho mượt hơn

        operation.allowSceneActivation = true;

        // Tự ẩn sau khi vào scene mới
        StartCoroutine(HideLoadingUIAfterDelay());
    }

    IEnumerator HideLoadingUIAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
    }
}