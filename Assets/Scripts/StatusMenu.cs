using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// Tab으로 여닫는 상태창(책). 값은 GameHUD에서 읽어 온다
public class StatusMenu : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform book;
    [SerializeField] private GameHUD hud;

    [Header("왼쪽 페이지")]
    [SerializeField] private TMP_Text locationText;
    [SerializeField] private Image[] heartImages;
    [SerializeField] private TMP_Text conditionText;

    [Header("오른쪽 페이지")]
    [SerializeField] private TMP_Text instrumentNameText;
    [SerializeField] private TMP_Text recordText;

    [Header("하트 스프라이트")]
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartHalf;
    [SerializeField] private Sprite heartEmpty;

    [SerializeField] private float openTime = 0.15f;
    [SerializeField] private float closedScale = 0.92f;

    private Coroutine anim;

    private void Awake()
    {
        IsOpen = false;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        book.localScale = Vector3.one * closedScale;
    }

    private void OnDestroy()
    {
        IsOpen = false;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return;

        if (keyboard.tabKey.wasPressedThisFrame) SetOpen(!IsOpen);
        else if (IsOpen && keyboard.escapeKey.wasPressedThisFrame) SetOpen(false);
    }

    public void SetOpen(bool open)
    {
        if (open == IsOpen) return;
        IsOpen = open;
        if (open) Refresh();
        group.blocksRaycasts = open;

        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(Animate(open));
    }

    private IEnumerator Animate(bool open)
    {
        float fromA = group.alpha;
        float toA = open ? 1f : 0f;
        float fromS = book.localScale.x;
        float toS = open ? 1f : closedScale;
        float duration = open ? openTime : openTime * 0.7f;

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = t / duration;
            k = 1f - (1f - k) * (1f - k);
            group.alpha = Mathf.Lerp(fromA, toA, k);
            book.localScale = Vector3.one * Mathf.Lerp(fromS, toS, k);
            yield return null;
        }
        group.alpha = toA;
        // 다 열린 상태는 배율 1로 고정해 픽셀이 번지지 않게 한다
        book.localScale = Vector3.one * toS;
        anim = null;
    }

    private void Refresh()
    {
        if (hud == null) return;

        if (locationText != null) locationText.text = "현재 위치 · " + hud.MapName;
        if (instrumentNameText != null) instrumentNameText.text = hud.InstrumentName;

        if (heartImages != null)
        {
            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] == null) continue;
                heartImages[i].gameObject.SetActive(i < hud.MaxHearts);
                int filled = hud.CurrentHalfHearts - i * 2;
                heartImages[i].sprite = filled >= 2 ? heartFull : filled == 1 ? heartHalf : heartEmpty;
            }
        }

        if (conditionText != null)
        {
            string c = ColorUtility.ToHtmlStringRGB(hud.ConditionColor);
            conditionText.text = $"<color=#{c}>●</color> {hud.Condition}";
        }

        if (recordText != null)
        {
            recordText.text = GameState.BossCleared ? "탑의 보스  <color=#3E7A3A>클리어</color>" : "탑의 보스  아직 도전 전";
        }
    }
}
