using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text lineText;

    private DialogueData currentData;
    private int currentIndex;

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!IsPlaying) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            ShowNextLine();
        }
    }

    public void StartDialogue(DialogueData data)
    {
        if (data == null || data.Lines == null || data.Lines.Length == 0) return;
        if (IsPlaying) return;

        currentData = data;
        currentIndex = 0;
        IsPlaying = true;

        dialoguePanel.SetActive(true);
        speakerText.text = currentData.SpeakerName;
        lineText.text = currentData.Lines[currentIndex];
    }

    private void ShowNextLine()
    {
        currentIndex++;

        if (currentIndex >= currentData.Lines.Length)
        {
            EndDialogue();
            return;
        }

        lineText.text = currentData.Lines[currentIndex];
    }

    private void EndDialogue()
    {
        IsPlaying = false;
        currentData = null;
        currentIndex = 0;
        dialoguePanel.SetActive(false);
    }
}