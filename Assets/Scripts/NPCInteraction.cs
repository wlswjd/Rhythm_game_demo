using UnityEngine;

public class NPCInteraction : Interactable
{
    [SerializeField] private DialogueData dialogueData;

    public override void Interact()
    {
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.StartDialogue(dialogueData);
    }
}
