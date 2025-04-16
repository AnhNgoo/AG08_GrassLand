using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

// Quản lý NPC và hiển thị cutscene thoại
public class NPC : MonoBehaviour
{
    [SerializeField] private Dialogue dialogue; // Dữ liệu câu thoại
    [SerializeField] private GameObject cutscenePanel; // Panel hiển thị cutscene
    [SerializeField] private TextMeshProUGUI dialogueText; // Text hiển thị câu thoại
    [SerializeField] private Button continueButton; // Nút chuyển câu thoại

    private PlayerController player; // Tham chiếu đến player
    private bool isCutsceneActive = false; // Trạng thái cutscene
    private int currentSentenceIndex = 0; // Chỉ số câu thoại hiện tại
    private bool hasPlayedCutscene = false; // Đã chơi cutscene?
    private bool isTyping = false; // Trạng thái đang đánh máy
    private Coroutine typingCoroutine; // Coroutine để dừng hiệu ứng đánh máy

    void Start()
    {
        // Ẩn panel và nút, gán sự kiện cho nút
        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false); // Ẩn nút ban đầu
            continueButton.onClick.AddListener(OnContinueButtonClicked); // Gán hàm xử lý nút
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kích hoạt cutscene khi player vào vùng
        if (other.CompareTag("Player") && !isCutsceneActive && !hasPlayedCutscene)
        {
            player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                StartCutscene();
                hasPlayedCutscene = true; // Chỉ chơi cutscene một lần
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Kết thúc cutscene nếu player rời vùng
        if (other.CompareTag("Player") && isCutsceneActive)
            EndCutscene();
    }

    private void StartCutscene()
    {
        isCutsceneActive = true;
        currentSentenceIndex = 0;
        if (cutscenePanel != null)
            cutscenePanel.SetActive(true); // Hiển thị panel
        if (player != null)
            player.SetCanMove(false); // Ngăn player di chuyển
        ShowNextSentence(); // Hiển thị câu đầu tiên
    }

    private void ShowNextSentence()
    {
        // Chuyển sang câu tiếp theo
        if (currentSentenceIndex < dialogue.sentences.Length)
        {
            if (dialogueText != null)
            {
                if (continueButton != null)
                    continueButton.gameObject.SetActive(false); // Ẩn nút khi bắt đầu đánh máy
                typingCoroutine = StartCoroutine(TypeSentence(dialogue.sentences[currentSentenceIndex])); // Bắt đầu hiệu ứng đánh máy
            }
            currentSentenceIndex++;
        }
        else
            EndCutscene(); // Kết thúc nếu hết câu
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        // Hiển thị từng chữ
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); // Delay giữa các chữ
        }
        isTyping = false;
        if (continueButton != null)
            continueButton.gameObject.SetActive(true); // Hiển thị nút khi đánh máy xong
    }

    private void OnContinueButtonClicked()
    {
        if (isTyping)
        {
            // Nếu đang đánh máy, dừng hiệu ứng và hiển thị toàn bộ câu
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            dialogueText.text = dialogue.sentences[currentSentenceIndex - 1]; // Hiển thị toàn bộ câu hiện tại
            isTyping = false;
            if (continueButton != null)
                continueButton.gameObject.SetActive(true); // Hiển thị nút để chuyển câu tiếp theo
        }
        else
        {
            // Nếu không còn đánh máy, chuyển sang câu tiếp theo
            ShowNextSentence();
        }
    }

    private void EndCutscene()
    {
        isCutsceneActive = false;
        currentSentenceIndex = 0;
        if (cutscenePanel != null)
            cutscenePanel.SetActive(false); // Ẩn panel
        if (continueButton != null)
            continueButton.gameObject.SetActive(false); // Ẩn nút
        if (player != null)
            player.SetCanMove(true); // Cho phép player di chuyển
    }
}