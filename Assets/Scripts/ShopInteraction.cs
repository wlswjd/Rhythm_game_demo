using UnityEngine;

// 가판대. 상점 UI가 생기기 전까지는 안내 문구만 띄운다
public class ShopInteraction : Interactable
{
    [SerializeField] private string notReadyMessage = "아직 물건을 팔지 않는다";

    public override void Interact()
    {
        if (InteractPrompt.Instance != null)
        {
            InteractPrompt.Instance.ShowMessage(notReadyMessage, PromptWorldPosition);
        }
    }
}
