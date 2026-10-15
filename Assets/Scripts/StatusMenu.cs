using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// Tab으로 여닫는 상태창(책). 값은 GameHUD에서 읽어 온다
// 오른쪽 꼬리표로 상태 / 악기 / 악보 / 설정 페이지를 넘긴다
public class StatusMenu : MonoBehaviour
{
    [System.Serializable]
    private class BookTab
    {
        public Button button;
        public Image tabImage;
        public Color color = Color.white;
        [Tooltip("이 꼬리표를 골랐을 때 켜지는 페이지 내용들")]
        public GameObject[] contents;
    }

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

    [Header("꼬리표")]
    [SerializeField] private BookTab[] tabs;
    [Tooltip("선택된 꼬리표가 책 밖으로 더 튀어나오는 길이")]
    [SerializeField] private float selectedPopOut = 6f;

    [SerializeField] private float openTime = 0.15f;
    [SerializeField] private float closedScale = 0.92f;

    private Coroutine anim;
    private int currentTab;
    private float[] tabRestX;

    private void Awake()
    {
        IsOpen = false;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
        book.localScale = Vector3.one * closedScale;

        tabRestX = new float[tabs.Length];
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i;
            tabRestX[i] = ((RectTransform)tabs[i].button.transform).anchoredPosition.x;
            tabs[i].button.onClick.AddListener(() => SelectTab(index));
        }
        SelectTab(0);
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
        else if (IsOpen)
        {
            if (keyboard.upArrowKey.wasPressedThisFrame) SelectTab((currentTab + tabs.Length - 1) % tabs.Length);
            if (keyboard.downArrowKey.wasPressedThisFrame) SelectTab((currentTab + 1) % tabs.Length);
            for (int i = 0; i < tabs.Length && i < 9; i++)
            {
                if (keyboard[Key.Digit1 + i].wasPressedThisFrame) SelectTab(i);
            }
        }
    }

    public void SelectTab(int index)
    {
        currentTab = index;
        for (int i = 0; i < tabs.Length; i++)
        {
            bool on = i == index;
            foreach (var c in tabs[i].contents) if (c != null) c.SetActive(on);

            // 고른 꼬리표는 더 튀어나오고 밝게, 나머지는 살짝 어둡게
            var rt = (RectTransform)tabs[i].button.transform;
            rt.anchoredPosition = new Vector2(tabRestX[i] + (on ? selectedPopOut : 0f), rt.anchoredPosition.y);
            Color c0 = tabs[i].color;
            tabs[i].tabImage.color = on ? c0 : new Color(c0.r * 0.8f, c0.g * 0.8f, c0.b * 0.8f, 1f);
        }
    }

    public void SetOpen(bool open)
    {
        if (open == IsOpen) return;
        IsOpen = open;
        if (open) Refresh();
        // 꼬리표·슬라이더 클릭은 열려 있을 때만 받는다
        group.blocksRaycasts = open;
        group.interactable = open;

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
