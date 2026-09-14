using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData warningDialogue;

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggered) return;

        triggered = true;

        if (warningDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(warningDialogue);
        }

        Debug.Log("보스전 진입 트리거 발동");
    }
}