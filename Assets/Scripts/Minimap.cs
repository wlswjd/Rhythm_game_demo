using System.Collections.Generic;
using UnityEngine;

// 구워 둔 미니맵 이미지 위에 플레이어·NPC·보스 입구 표시를 올린다
public class Minimap : MonoBehaviour
{
    [SerializeField] private RectTransform mapImage;
    [SerializeField] private CanvasGroup group;
    [Tooltip("미니맵 이미지가 덮는 월드 영역 (MinimapBaker와 같아야 함)")]
    [SerializeField] private Rect worldRect = new Rect(-20f, -20f, 40f, 48f);

    [Header("표시")]
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private RectTransform markerTemplate;
    [SerializeField] private Color npcColor = new Color(1f, 0.6f, 0.2f);
    [SerializeField] private Color bossColor = new Color(0.9f, 0.2f, 0.25f);
    [SerializeField] private float blinkInterval = 0.4f;

    private Transform player;
    private readonly List<(Transform target, RectTransform marker)> markers = new List<(Transform, RectTransform)>();

    private void Start()
    {
        var p = FindFirstObjectByType<PlayerMovement>();
        if (p != null) player = p.transform;

        foreach (var npc in FindObjectsByType<NPCInteraction>(FindObjectsSortMode.None)) AddMarker(npc.transform, npcColor);
        var boss = FindFirstObjectByType<BossTrigger>();
        if (boss != null) AddMarker(boss.transform, bossColor);

        markerTemplate.gameObject.SetActive(false);
        playerMarker.SetAsLastSibling();
    }

    private void AddMarker(Transform target, Color color)
    {
        var m = Instantiate(markerTemplate, markerTemplate.parent);
        m.name = "Marker_" + target.name;
        m.GetComponent<UnityEngine.UI.Image>().color = color;
        m.gameObject.SetActive(true);
        markers.Add((target, m));
    }

    private void LateUpdate()
    {
        bool hidden = StatusMenu.IsOpen;
        if (group != null) group.alpha = hidden ? 0f : 1f;
        if (hidden) return;

        foreach (var (target, marker) in markers)
        {
            if (target == null) continue;
            marker.anchoredPosition = ToMap(target.position);
        }

        if (player != null)
        {
            playerMarker.anchoredPosition = ToMap(player.position);
            // 깜빡여서 다른 표시와 구분
            bool on = Mathf.FloorToInt(Time.unscaledTime / blinkInterval) % 2 == 0;
            playerMarker.gameObject.SetActive(on || blinkInterval <= 0f);
        }
    }

    private Vector2 ToMap(Vector3 world)
    {
        Vector2 size = mapImage.rect.size;
        float u = (world.x - worldRect.x) / worldRect.width;
        float v = (world.y - worldRect.y) / worldRect.height;
        // 픽셀이 번지지 않게 정수 좌표로 맞춘다
        return new Vector2(Mathf.Round(u * size.x), Mathf.Round(v * size.y));
    }
}
