using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("맵 이름 표지판 (우측 상단)")]
    [SerializeField] private CanvasGroup mapNameGroup;
    [SerializeField] private TMP_Text mapNameText;
    [SerializeField] private string mapName = "태초마을";
    [SerializeField] private bool showMapNameOnStart = true;
    [SerializeField] private float mapNameFadeIn = 0.4f;
    [SerializeField] private float mapNameHold = 1f;
    [SerializeField] private float mapNameFadeOut = 0.8f;
    [SerializeField] private float mapNameDrop = 12f;

    [Header("플레이어 상태 (좌측 하단)")]
    [SerializeField] private CanvasGroup statusGroup;
    [SerializeField] private TMP_Text instrumentText;
    [SerializeField] private TMP_Text conditionText;

    [Header("체력 하트")]
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartHalf;
    [SerializeField] private Sprite heartEmpty;

    // 임시 값. 장비·전투 시스템이 생기면 그쪽에서 Set 함수로 넣는다
    [SerializeField] private string instrumentName = "낡은 류트";
    [SerializeField] private int maxHearts = 5;
    [Tooltip("하트 반 칸 단위. 10이면 하트 5개가 꽉 참")]
    [SerializeField] private int currentHalfHearts = 10;
    [SerializeField] private string condition = "평온";
    [SerializeField] private Color conditionColor = new Color(0.36f, 0.6f, 0.3f);

    private Coroutine mapNameRoutine;
    private Vector2 mapNameRestPos;

    private void Awake()
    {
        if (mapNameGroup != null)
        {
            mapNameRestPos = ((RectTransform)mapNameGroup.transform).anchoredPosition;
            mapNameGroup.alpha = 0f;
        }
    }

    private void Start()
    {
        Refresh();
        if (showMapNameOnStart) ShowMapName(mapName);
    }

    private void Update()
    {
        // 대사창이 화면 하단을 덮으므로 대사 중에는 상태창을 숨긴다. 상태 메뉴가 열려 있을 때도 숨긴다
        if (statusGroup == null) return;
        bool talking = DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying;
        statusGroup.alpha = talking || StatusMenu.IsOpen ? 0f : 1f;
    }

    public string MapName => mapName;
    public string InstrumentName => instrumentName;
    public int MaxHearts => maxHearts;
    public int CurrentHalfHearts => currentHalfHearts;
    public string Condition => condition;
    public Color ConditionColor => conditionColor;

    // 씬 진입, 건물에서 나왔을 때 등 지역이 바뀌는 순간에 호출한다
    public void ShowMapName(string value)
    {
        mapName = value;
        Refresh();
        if (mapNameGroup == null) return;
        if (mapNameRoutine != null) StopCoroutine(mapNameRoutine);
        mapNameRoutine = StartCoroutine(MapNameRoutine());
    }

    private IEnumerator MapNameRoutine()
    {
        var rt = (RectTransform)mapNameGroup.transform;
        Vector2 from = mapNameRestPos + new Vector2(0f, mapNameDrop);

        for (float t = 0f; t < mapNameFadeIn; t += Time.unscaledDeltaTime)
        {
            float k = t / mapNameFadeIn;
            mapNameGroup.alpha = k;
            // 위에서 살짝 내려오며 나타남. 픽셀이 번지지 않게 정수 좌표로 맞춘다
            Vector2 p = Vector2.Lerp(from, mapNameRestPos, 1f - (1f - k) * (1f - k));
            rt.anchoredPosition = new Vector2(Mathf.Round(p.x), Mathf.Round(p.y));
            yield return null;
        }
        mapNameGroup.alpha = 1f;
        rt.anchoredPosition = mapNameRestPos;

        yield return new WaitForSecondsRealtime(mapNameHold);

        for (float t = 0f; t < mapNameFadeOut; t += Time.unscaledDeltaTime)
        {
            mapNameGroup.alpha = 1f - t / mapNameFadeOut;
            yield return null;
        }
        mapNameGroup.alpha = 0f;
        mapNameRoutine = null;
    }

    public void SetInstrument(string value)
    {
        instrumentName = value;
        Refresh();
    }

    public void SetHearts(int halfHearts, int max)
    {
        maxHearts = Mathf.Max(1, max);
        currentHalfHearts = Mathf.Clamp(halfHearts, 0, maxHearts * 2);
        Refresh();
    }

    public void SetCondition(string value, Color color)
    {
        condition = value;
        conditionColor = color;
        Refresh();
    }

    private void Refresh()
    {
        if (mapNameText != null) mapNameText.text = mapName;
        if (instrumentText != null) instrumentText.text = instrumentName;

        if (heartImages != null)
        {
            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] == null) continue;
                heartImages[i].gameObject.SetActive(i < maxHearts);

                int filled = currentHalfHearts - i * 2;
                heartImages[i].sprite = filled >= 2 ? heartFull : filled == 1 ? heartHalf : heartEmpty;
            }
        }

        if (conditionText != null)
        {
            string c = ColorUtility.ToHtmlStringRGB(conditionColor);
            conditionText.text = $"<color=#{c}>●</color> {condition}";
        }
    }

    private void OnValidate()
    {
        maxHearts = Mathf.Max(1, maxHearts);
        currentHalfHearts = Mathf.Clamp(currentHalfHearts, 0, maxHearts * 2);
        Refresh();
    }
}
