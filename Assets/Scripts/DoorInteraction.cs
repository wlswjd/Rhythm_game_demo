using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 건물 문. 건물 스프라이트에 그려진 문 위에 같은 그림의 애니메이션 프레임을 겹쳐 두고, E키를 누르면 열린다
public class DoorInteraction : Interactable
{
    [SerializeField] private SpriteRenderer doorRenderer;
    [Tooltip("0 닫힘 → 마지막 완전히 열림")]
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameTime = 0.07f;

    [Tooltip("비어 있으면 아직 내부가 없는 건물. 문만 열렸다 닫힌다")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private string lockedMessage = "아직 들어갈 수 없다";
    [SerializeField] private float stayOpenTime = 0.8f;

    private bool busy;

    public override bool CanInteract => !busy;

    public override void Interact()
    {
        if (busy) return;
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        busy = true;

        for (int i = 1; i < frames.Length; i++)
        {
            doorRenderer.sprite = frames[i];
            yield return new WaitForSeconds(frameTime);
        }

        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
            yield break;
        }

        if (InteractPrompt.Instance != null)
        {
            InteractPrompt.Instance.ShowMessage(lockedMessage, PromptWorldPosition, stayOpenTime + frameTime * frames.Length);
        }
        yield return new WaitForSeconds(stayOpenTime);

        for (int i = frames.Length - 2; i >= 0; i--)
        {
            doorRenderer.sprite = frames[i];
            yield return new WaitForSeconds(frameTime);
        }

        busy = false;
    }
}
