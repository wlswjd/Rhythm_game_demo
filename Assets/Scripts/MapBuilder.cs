using UnityEngine;
using UnityEngine.Tilemaps;

public class MapBuilder : MonoBehaviour
{
    [System.Serializable]
    public struct Area
    {
        public string name;
        public Vector2Int origin;
        public Vector2Int size;
    }

    [SerializeField] private Tilemap groundMap;
    [SerializeField] private Tilemap collisionMap;
    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private Area[] areas =
    {
        new Area { name = "Village", origin = new Vector2Int(-20, -15), size = new Vector2Int(40, 30) },
        new Area { name = "Forest",  origin = new Vector2Int(40, -15),  size = new Vector2Int(40, 30) },
    };

    [ContextMenu("Build Base Tiles")]
    private void Build()
    {
        if (groundMap == null || collisionMap == null || groundTile == null || wallTile == null)
        {
            Debug.LogError("MapBuilder: 슬롯이 비어 있습니다.");
            return;
        }

#if UNITY_EDITOR
        UnityEditor.Undo.RegisterCompleteObjectUndo(new Object[] { groundMap, collisionMap }, "Build Base Tiles");
#endif

        groundMap.ClearAllTiles();
        collisionMap.ClearAllTiles();

        foreach (Area a in areas)
        {
            for (int x = 0; x < a.size.x; x++)
            {
                for (int y = 0; y < a.size.y; y++)
                {
                    Vector3Int cell = new Vector3Int(a.origin.x + x, a.origin.y + y, 0);
                    groundMap.SetTile(cell, groundTile);

                    bool isEdge = x == 0 || y == 0 || x == a.size.x - 1 || y == a.size.y - 1;
                    if (isEdge) collisionMap.SetTile(cell, wallTile);
                }
            }
        }

#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        Debug.Log($"[MapBuilder] {areas.Length}개 구역 생성 완료");
    }
}