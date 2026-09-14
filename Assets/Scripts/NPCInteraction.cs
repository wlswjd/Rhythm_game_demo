using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private DialogueData dialogueData;

    private bool playerInRange;

    private void Update()
    {
        if (!playerInRange) return;
        if (DialogueManager.Instance == null) return;
        if (DialogueManager.Instance.IsPlaying) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.eKey.wasPressedThisFrame)
        {
            DialogueManager.Instance.StartDialogue(dialogueData);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}