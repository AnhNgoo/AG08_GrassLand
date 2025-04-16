using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

// Quản lý màn hình loading khi chuyển scene
public class LoadingManager : MonoBehaviour
{
    public GameObject loadingScreen; // UI màn hình loading
    public Slider progressBar; // Thanh tiến độ loading
    public TextMeshProUGUI progressText; // Text hiển thị phần trăm

    public void LoadScene(string sceneName)
    {
        // Bắt đầu chuyển scene
        Time.timeScale = 1f; // Đặt lại thời gian về bình thường
        StartCoroutine(LoadSceneAsync(sceneName)); // Chạy coroutine tải scene
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingScreen.SetActive(true); // Hiển thị màn hình loading

        // Tải scene bất đồng bộ
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // Chưa cho phép kích hoạt scene

        float displayProgress = 0f; // Tiến độ hiển thị trên UI

        // Cập nhật tiến độ cho đến khi gần hoàn tất (0.9)
        while (operation.progress < 0.9f)
        {
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f); // Chuẩn hóa tiến độ
            displayProgress = Mathf.MoveTowards(displayProgress, realProgress, Time.deltaTime * 0.5f); // Mượt hóa tiến độ
            progressBar.value = displayProgress; // Cập nhật thanh tiến độ
            if (progressText != null)
                progressText.text = "Loading " + (displayProgress * 100f).ToString("F0") + "%"; // Cập nhật text
            yield return null;
        }

        // Mượt hóa tiến độ đến 100%
        while (displayProgress < 1f)
        {
            displayProgress = Mathf.MoveTowards(displayProgress, 1f, Time.deltaTime * 0.5f);
            progressBar.value = displayProgress;
            if (progressText != null)
                progressText.text = "Loading " + (displayProgress * 100f).ToString("F0") + "%";
            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // Delay nhẹ để mượt hơn
        operation.allowSceneActivation = true; // Kích hoạt scene mới

        // Ẩn UI sau khi tải xong
        StartCoroutine(HideLoadingUIAfterDelay());
    }

    IEnumerator HideLoadingUIAfterDelay()
    {
        yield return new WaitForSeconds(1f); // Chờ 1 giây
        if (loadingScreen != null)
            loadingScreen.SetActive(false); // Ẩn màn hình loading
    }
}