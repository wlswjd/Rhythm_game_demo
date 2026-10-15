using UnityEngine;
using UnityEngine.SceneManagement;

public class BossTrigger : Interactable
{
    [SerializeField] private DialogueData warningDialogue;
    [SerializeField] private DialogueData clearedDialogue;
    [SerializeField] private string bossSceneName = "Scene_Boss";

    private bool warned;

    public override bool CanInteract => !GameState.BossCleared;

    public override void Interact()
    {
        SceneManager.LoadScene(bossSceneName);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if (!other.CompareTag("Player")) return;

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

    protected override void OnTriggerExit2D(Collider2D other)
    {
        base.OnTriggerExit2D(other);
        if (!other.CompareTag("Player")) return;
        warned = false;
    }
}
