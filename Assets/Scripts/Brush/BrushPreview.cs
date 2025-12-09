using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BrushPreview : MonoBehaviour
{
    [SerializeField] new Camera camera;
    [SerializeField] TileBrush brush;
    [SerializeField] SO_Tiles previweTiles;
    [SerializeField] SO_Tiles emptyTiles;
    [SerializeField] Slider BrushSizeSlider;
    [SerializeField] int offset;
    [SerializeField] List<Tilemap> tilemaps;

    private void Update()
    {
        if (IsPointerOverUI())
        {
            CircleDraw(emptyTiles, offset * 2 + (int)BrushSizeSlider.value);
            return;
        }
        CircleDraw(emptyTiles, (int)BrushSizeSlider.value + offset);
        CircleDraw(previweTiles, (int)BrushSizeSlider.value);
    }
    
    private Tilemap FindTargetTilemap(Vector3Int pos, SO_Tiles pRuleTile)
    {
        if (tilemaps == null || tilemaps.Count == 0) return null;

        if (pRuleTile != null && pRuleTile.layerMask != 0)
        {
            for (int i = tilemaps.Count - 1; i >= 0; i--)
            {
                var tm = tilemaps[i];
                if (tm == null) continue;
                if ((pRuleTile.layerMask & (1 << tm.gameObject.layer)) != 0)
                    return tm;
            }
        }

        if (pRuleTile == EraseTile)
        {
            for (int i = tilemaps.Count - 1; i >= 0; i--)
            {
                var tm = tilemaps[i];
                if (tm == null) continue;
                if (tm.GetTile(pos) != null) return tm;
            }
        }

        for (int i = tilemaps.Count - 1; i >= 0; i--)
        {
            var tm = tilemaps[i];
            if (tm != null) return tm;
        }

        return null;
    }
    
    private void CircleDraw(SO_Tiles pRuleTile, int pBrushSize, bool pIsCircle = false)
    {
        if (tilemaps == null || tilemaps.Count == 0 || camera == null) return;

        Vector3 mouse = Input.mousePosition;
        mouse.z = -camera.transform.position.z;
        Vector3 worldPos = camera.ScreenToWorldPoint(mouse);
        Vector3Int midCell = tilemaps[0].WorldToCell(new Vector3(worldPos.x, worldPos.y, 0f));

        int size = Mathf.Max(0, pBrushSize);
        int rSq = size * size;

        if (pRuleTile == EraseTile && target == null)
        {
            target = FindTargetTilemap(midCell, pRuleTile);
        }

        for (int dx = -size; dx <= size; dx++)
        {
            for (int dy = -size; dy <= size; dy++)
            {
                if (dx * dx + dy * dy <= rSq)
                {
                    Vector3Int cellPos = new Vector3Int(midCell.x + dx, midCell.y + dy, midCell.z);

                    if (target == null)
                        target = FindTargetTilemap(cellPos, pRuleTile);
                    if (target == null) continue;

                    target.SetTile(cellPos, pRuleTile != null ? pRuleTile.RuleTiles : null);
                    if (pRuleTile != null)
                        target.SetColor(cellPos, pRuleTile.color);
                    else
                        target.SetColor(cellPos, Color.clear);
                }
            }
        }
    }
    
    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}
