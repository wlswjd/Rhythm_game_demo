using UnityEngine;

// E키 상호작용 대상의 공통 부모. 같은 오브젝트에 Is Trigger 콜라이더가 있어야 한다
public abstract class Interactable : MonoBehaviour
{
    [SerializeField] private string promptLabel = "대화";
    [Tooltip("안내 표시가 뜨는 위치. 이 오브젝트 기준 월드 좌표 오프셋")]
    [SerializeField] private Vector2 promptOffset = new Vector2(0f, 1.2f);

    public string PromptLabel => promptLabel;
    public Vector3 PromptWorldPosition => transform.position + (Vector3)promptOffset;

    public virtual bool CanInteract => true;

    public abstract void Interact();

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        var interactor = other.GetComponentInParent<PlayerInteractor>();
        if (interactor != null) interactor.Register(this);
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        var interactor = other.GetComponentInParent<PlayerInteractor>();
        if (interactor != null) interactor.Unregister(this);
    }
}
