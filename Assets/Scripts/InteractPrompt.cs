using UnityEngine;
using TMPro;

// 대상 머리 위에 뜨는 "[E] 대화" 안내. 잠깐 띄우는 알림 문구도 같은 자리에 쓴다
public class InteractPrompt : MonoBehaviour
{
    public static InteractPrompt Instance { get; private set; }

    [SerializeField] private RectTransform panel;
    [SerializeField] private GameObject keyCap;
    [SerializeField] private TMP_Text labelText;

    private RectTransform canvasRect;
    private Interactable target;
    private string message;
    private Vector3 messageWorldPos;
    private float messageUntil;

    private void Awake()
    {
        Instance = this;
        canvasRect = (RectTransform)GetComponentInParent<Canvas>().transform;
        panel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void SetTarget(Interactable value)
    {
        target = value;
    }

    // 아직 구현되지 않은 기능 등에 짧게 띄우는 문구
    public void ShowMessage(string text, Vector3 worldPos, float seconds = 1.5f)
    {
        message = text;
        messageWorldPos = worldPos;
        messageUntil = Time.time + seconds;
    }

    private void LateUpdate()
    {
        bool showMessage = Time.time < messageUntil;
        if (!showMessage && target == null)
        {
            panel.gameObject.SetActive(false);
            return;
        }

        panel.gameObject.SetActive(true);
        keyCap.SetActive(!showMessage);
        labelText.text = showMessage ? message : target.PromptLabel;

        Vector3 world = showMessage ? messageWorldPos : target.PromptWorldPosition;
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector2 screen = cam.WorldToScreenPoint(world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local);
        // 픽셀이 번지지 않게 정수 좌표로 맞춘다
        panel.anchoredPosition = new Vector2(Mathf.Round(local.x), Mathf.Round(local.y));
    }
}
