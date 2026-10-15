using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

// 열려 있는 월드 씬을 미니맵 이미지로 굽는다. 맵을 고친 뒤 메뉴에서 다시 실행하면 된다
public static class MinimapBaker
{
    private const string OutputPath = "Assets/Sprites/UI/Minimap_World.png";
    private const int PixelsPerTile = 2;
    private static readonly RectInt MapRect = new RectInt(-20, -20, 40, 48);

    private static readonly Color Outside = new Color32(0x2a, 0x3d, 0x2e, 255);
    private static readonly Color Forest = new Color32(0x3b, 0x6e, 0x3f, 255);
    private static readonly Color TreeCanopy = new Color32(0x4a, 0x85, 0x48, 255);

    private static readonly Dictionary<Texture2D, Texture2D> readableCache = new Dictionary<Texture2D, Texture2D>();

    [MenuItem("Tools/Minimap/Bake World Minimap")]
    public static void Bake()
    {
        readableCache.Clear();
        int w = MapRect.width * PixelsPerTile;
        int h = MapRect.height * PixelsPerTile;
        var pixels = new Color[w * h];

        Tilemap ground = Find("Tilemap_Ground");
        Tilemap road = Find("Tilemap_Road");
        Tilemap collision = Find("Tilemap_Collision");

        // 1) 바닥과 길: 타일 스프라이트의 평균색, 충돌 칸은 숲색
        for (int cy = 0; cy < MapRect.height; cy++)
        {
            for (int cx = 0; cx < MapRect.width; cx++)
            {
                var cell = new Vector3Int(MapRect.x + cx, MapRect.y + cy, 0);
                Color c = Outside;
                Sprite s = SpriteAt(road, cell) ?? SpriteAt(ground, cell);
                if (s != null) c = Average(s);
                if (collision != null && collision.HasTile(cell)) c = Forest;
                Fill(pixels, w, cx * PixelsPerTile, cy * PixelsPerTile, PixelsPerTile, PixelsPerTile, c);
            }
        }

        // 2) 숲 안쪽 나무: 밑동 위쪽에 둥근 수관
        var forestRoot = GameObject.Find("Forest");
        if (forestRoot != null)
        {
            foreach (var sr in forestRoot.GetComponentsInChildren<SpriteRenderer>())
            {
                if (sr.sprite == null || sr.sprite.rect.height < 40) continue;
                Vector2 p = sr.transform.position;
                float r = Mathf.Clamp(sr.sprite.rect.width / 16f * 0.45f, 0.8f, 2f);
                Disc(pixels, w, h, p + new Vector2(0f, r), r, TreeCanopy);
            }
        }

        // 3) 건물: 밑동부터 스프라이트 높이의 절반을 지붕색(윗부분 평균색)으로
        var buildings = GameObject.Find("Buildings");
        if (buildings != null)
        {
            foreach (Transform t in buildings.transform)
            {
                var sr = t.GetComponent<SpriteRenderer>();
                if (sr == null || sr.sprite == null) continue;
                Bounds b = sr.bounds;
                Color roof = Average(sr.sprite, 0.55f, 1f);
                Color dark = roof * 0.7f; dark.a = 1f;
                float top = b.min.y + b.size.y * 0.5f;
                Rect(pixels, w, h, b.min.x + 0.2f, t.position.y, b.max.x - 0.2f, top, roof, dark);
            }
        }

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(OutputPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        foreach (var r in readableCache.Values) Object.DestroyImmediate(r);
        readableCache.Clear();

        AssetDatabase.ImportAsset(OutputPath);
        var importer = (TextureImporter)AssetImporter.GetAtPath(OutputPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 16;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
        Debug.Log($"미니맵 굽기 완료: {OutputPath} ({w}x{h})");
    }

    private static Tilemap Find(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go.GetComponent<Tilemap>() : null;
    }

    private static Sprite SpriteAt(Tilemap map, Vector3Int cell)
    {
        if (map == null) return null;
        return map.GetSprite(cell);
    }

    private static Color Average(Sprite s, float fromV = 0f, float toV = 1f)
    {
        Texture2D readable = Readable(s.texture);
        Rect r = s.textureRect;
        int y0 = Mathf.FloorToInt(r.y + r.height * fromV);
        int y1 = Mathf.CeilToInt(r.y + r.height * toV);
        Color sum = Color.clear;
        int n = 0;
        for (int y = y0; y < y1; y++)
        {
            for (int x = (int)r.x; x < (int)(r.x + r.width); x++)
            {
                Color c = readable.GetPixel(x, y);
                if (c.a < 0.5f) continue;
                sum += c;
                n++;
            }
        }
        if (n == 0) return Outside;
        Color avg = sum / n;
        avg.a = 1f;
        return avg;
    }

    // 임포트 설정을 건드리지 않고 픽셀을 읽기 위해 RenderTexture로 복사한다
    private static Texture2D Readable(Texture2D src)
    {
        if (readableCache.TryGetValue(src, out var cached)) return cached;
        var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        Graphics.Blit(src, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var copy = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
        copy.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        readableCache[src] = copy;
        return copy;
    }

    private static void Fill(Color[] px, int w, int x, int y, int fw, int fh, Color c)
    {
        for (int j = 0; j < fh; j++)
            for (int i = 0; i < fw; i++)
                px[(y + j) * w + (x + i)] = c;
    }

    private static Vector2Int ToPixel(Vector2 world)
    {
        return new Vector2Int(
            Mathf.FloorToInt((world.x - MapRect.x) * PixelsPerTile),
            Mathf.FloorToInt((world.y - MapRect.y) * PixelsPerTile));
    }

    private static void Disc(Color[] px, int w, int h, Vector2 center, float radius, Color c)
    {
        Vector2Int min = ToPixel(center - Vector2.one * radius);
        Vector2Int max = ToPixel(center + Vector2.one * radius);
        Vector2 cp = (center - (Vector2)MapRect.position) * PixelsPerTile;
        float rp = radius * PixelsPerTile;
        for (int y = min.y; y <= max.y; y++)
            for (int x = min.x; x <= max.x; x++)
            {
                if (x < 0 || y < 0 || x >= w || y >= h) continue;
                if ((new Vector2(x + 0.5f, y + 0.5f) - cp).sqrMagnitude > rp * rp) continue;
                px[y * w + x] = c;
            }
    }

    // 테두리 1px을 어둡게 해서 건물이 바닥과 구분되게 한다
    private static void Rect(Color[] px, int w, int h, float x0, float y0, float x1, float y1, Color fill, Color edge)
    {
        Vector2Int a = ToPixel(new Vector2(x0, y0));
        Vector2Int b = ToPixel(new Vector2(x1, y1));
        for (int y = a.y; y <= b.y; y++)
            for (int x = a.x; x <= b.x; x++)
            {
                if (x < 0 || y < 0 || x >= w || y >= h) continue;
                bool border = x == a.x || x == b.x || y == a.y || y == b.y;
                px[y * w + x] = border ? edge : fill;
            }
    }
}
