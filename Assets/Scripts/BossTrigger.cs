using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData warningDialogue;
    [SerializeField] private DialogueData clearedDialogue;
    [SerializeField] private string bossSceneName = "Scene_Boss";

    private bool playerInRange;
    private bool warned;

    private void Update()
    {
        if (!playerInRange) return;
        if (GameState.BossCleared) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.eKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(bossSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;

        if (DialogueManager.Instance == null) return;

        if (GameState.BossCleared)
        {
            if (clearedDialogue != null) DialogueManager.Instance.StartDialogue(clearedDialogue);
            return;
        }

        if (!warned && warningDialogue != null)
        {
            warned = true;
            DialogueManager.Instance.StartDialogue(warningDialogue);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        warned = false;
    }
}