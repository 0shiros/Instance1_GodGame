using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class BrushPreview : MonoBehaviour
{
    [SerializeField] new Camera camera;
    public Tilemap targetTilemap; // peut être assignée ou définie dynamiquement
    public Tilemap previewTilemap; // Tilemap pour l'aperçu (peut être null -> créé automatiquement)
    public TileBase brushTile; // Tile utilisée pour peindre
    public TileBase previewTile; // Tile utilisée pour l'aperçu (opaque/mono)
    public Color previewColor = new Color(1f, 1f, 1f, 0.5f);
    [Range(1, 50)] public int brushSize = 1;

    private bool isUpdatingPreview;
    private List<Vector3Int> currentPreviewPositions = new List<Vector3Int>();


    public void HidePreview()
    {
        isUpdatingPreview = false;
        ClearPreview();
    }
    public void ShowPreview()
    {
        if (previewTile == null || targetTilemap == null) return;
        EnsurePreviewTilemap();
        isUpdatingPreview = true;
        UpdatePreview();
    }
    
    public void SetTargetTilemap(Tilemap tilemap, int pSize, SO_Tiles pTile)
    {
        if (tilemap == targetTilemap && (pTile == null || pTile.RuleTiles == brushTile))
            return;

        targetTilemap = tilemap;
        brushSize = pSize;
        brushTile = pTile != null ? pTile.RuleTiles : null;
        previewTile = brushTile;
        isUpdatingPreview = brushTile != null;
        ShowPreview();
        
        if (targetTilemap != null)
            EnsurePreviewTilemap();
        if (!isUpdatingPreview)
            ClearPreview();
    }

    public void SetSize(int pSize)
    {
        brushSize = pSize;
    }

    private void Update()
    {
        if (!isUpdatingPreview) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (Camera.main == null && camera == null) return;

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (previewTile == null || targetTilemap == null) return;

        // Nettoie l'ancien aperçu
        ClearPreview();

        var midCell = GetMidCell();

        int size = Mathf.Max(0, brushSize);
        int rSq = size * size;

        for (int dx = -size; dx <= size; dx++)
        {
            for (int dy = -size; dy <= size; dy++)
            {
                if (dx * dx + dy * dy <= rSq)
                {
                    Vector3Int cellPos = new Vector3Int(midCell.x + dx, midCell.y + dy, midCell.z);

                    previewTilemap.SetTile(cellPos, previewTile);
                    previewTilemap.SetColor(cellPos, previewColor);
                    currentPreviewPositions.Add(cellPos);
                }
            }
        }
    }

    public void ClearPreview()
    {
        if (previewTilemap == null)
        {
            currentPreviewPositions.Clear();
            return;
        }

        foreach (var pos in currentPreviewPositions)
        {
            previewTilemap.SetTile(pos, null);
            previewTilemap.SetColor(pos, Color.clear);
        }

        currentPreviewPositions.Clear();
    }

    private void EnsurePreviewTilemap()
    {
        if (previewTilemap != null) return;

        GameObject go = new GameObject("BrushPreviewTilemap");
        if (targetTilemap != null)
        {
            if (targetTilemap.transform.parent != null)
                go.transform.SetParent(targetTilemap.transform.parent, false);
            else
                go.transform.SetParent(targetTilemap.transform, false);
        }

        previewTilemap = go.AddComponent<Tilemap>();
        var renderer = go.AddComponent<TilemapRenderer>();
        var targetRenderer = targetTilemap != null ? targetTilemap.GetComponent<TilemapRenderer>() : null;
        renderer.sortingOrder = (targetRenderer?.sortingOrder ?? 0) + 1;
        renderer.material = targetRenderer?.material;
    }

    private Vector3Int GetMidCell()
    {
        Vector3 cam = (camera != null) ? camera.transform.position : Camera.main.transform.position;
        Vector3 mouse = Input.mousePosition;
        mouse.z = -cam.z;
        Vector3 worldPos = (camera != null) ? camera.ScreenToWorldPoint(mouse) : Camera.main.ScreenToWorldPoint(mouse);
        Tilemap refMap = targetTilemap != null ? targetTilemap : previewTilemap;
        if (refMap == null) return Vector3Int.zero;
        Vector3Int midCell = refMap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0f));
        return midCell;
    }
}