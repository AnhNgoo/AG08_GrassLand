using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class NPC : MonoBehaviour
{
    [SerializeField] private Dialogue dialogue; // Gán Dialogue asset trong Inspector
    [SerializeField] private GameObject cutscenePanel; // Gán CutscenePanel trong Inspector
    [SerializeField] private Text dialogueText; // Gán DialogueText trong Inspector
    [SerializeField] private Button continueButton; // Gán ContinueButton trong Inspector

    private PlayerController player;
    private bool isCutsceneActive = false;
    private int currentSentenceIndex = 0;
    private bool hasPlayedCutscene = false;

    void Start()
    {
        // Ẩn cutscene panel ban đầu
        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(false);
        }
        // Gán sự kiện cho nút Continue
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ShowNextSentence);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCutsceneActive && !hasPlayedCutscene)
        {
            player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                StartCutscene();
                hasPlayedCutscene = true; // Đánh dấu cutscene đã chơi
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isCutsceneActive)
            {
                EndCutscene();
            }
        }
    }

    private void StartCutscene()
    {
        isCutsceneActive = true;
        currentSentenceIndex = 0;

        // Hiển thị cutscene panel
        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(true);
        }

        // Dừng player di chuyển
        if (player != null)
        {
            player.SetCanMove(false);
        }

        // Hiển thị câu thoại đầu tiên
        ShowNextSentence();
    }

    private void ShowNextSentence()
    {
        if (currentSentenceIndex < dialogue.sentences.Length)
        {
            if (dialogueText != null)
            {
                StartCoroutine(TypeSentence(dialogue.sentences[currentSentenceIndex]));
            }
            currentSentenceIndex++;
        }
        else
        {
            EndCutscene();
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); // Tốc độ đánh máy
        }
    }

    private void EndCutscene()
    {
        isCutsceneActive = false;
        currentSentenceIndex = 0;

        // Ẩn cutscene panel
        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(false);
        }

        // Cho phép player di chuyển lại
        if (player != null)
        {
            player.SetCanMove(true);
        }
    }
}