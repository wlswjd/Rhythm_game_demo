using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData warningDialogue;
    [SerializeField] private DialogueData clearedDialogue;
    [SerializeField] private string bossSceneName = "Scene_Boss";

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameState.BossCleared)
        {
            if (clearedDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(clearedDialogue);
            }
            return;
        }

        if (triggered) return;
        triggered = true;
        StartCoroutine(EnterBoss());
    }

    private IEnumerator EnterBoss()
    {
        if (warningDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(warningDialogue);
            while (DialogueManager.Instance.IsPlaying)
            {
                yield return null;
            }
        }

        SceneManager.LoadScene(bossSceneName);
    }
}