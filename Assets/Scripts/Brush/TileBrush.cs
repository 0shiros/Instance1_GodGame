using System;
using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TileBrush : MonoBehaviour
{
    [SerializeField] new Camera camera;
    [SerializeField] SO_Tiles EraseTile;
    [SerializeField] List<Tilemap> tilemaps;
    [SerializeField] SO_Tiles waterTile;
    [SerializeField] Tilemap waterTileMap;
    [SerializeField] Slider BrushSizeSlider;
    [SerializeField] int radius;
    [SerializeField] private Vector2Int mapBounds;
    [SerializeField] int minRadius;
    [SerializeField] int maxRadius;
    [SerializeField] private NavMeshSurface NavMesh;
    bool canDraw;
    bool canErase;
    private bool isEraseSelected;
    private Tilemap target;
    [SerializeField] private SO_Tiles currentTile;
    bool isSelected;
    ColorBlender colorBlender;

    private void OnDisable()
    {
        NavMesh.RemoveData();
    }

    private void Start()
    {
        colorBlender = ColorBlender.Instance;
        for (int x = Mathf.CeilToInt((mapBounds.x / 2) * -1); x < Mathf.CeilToInt(mapBounds.x / 2); x++)
        {
            for (int y = Mathf.CeilToInt((mapBounds.y / 2) * -1); y < Mathf.CeilToInt(mapBounds.x / 2); y++)
            {
                waterTileMap.SetTile(new Vector3Int(x, y, 0), waterTile.RuleTiles);
                waterTileMap.SetColor(new Vector3Int(x, y, 0), waterTile.color);
            }
        }
    }

    public void GetTile(SO_Tiles pTile)
    {
        currentTile = pTile;
        colorBlender.SetColorForTile(pTile);
        target = null;
    }

    public void TileSelected(bool pIsSelected)
    {
        isSelected = pIsSelected;
    }

    private void Update()
    {
        if (IsPointerOverUI() || !isSelected) return;

        if (canDraw)
            DrawTiles();
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

    public void CanDraw(InputAction.CallbackContext context)
    {
        if (currentTile == null || !isSelected) return;
        if (context.started) canDraw = true;
        if (context.canceled)
        {
            canDraw = false;
            target = null;
            if (NavMesh != null)
            {
                NavMesh.UpdateNavMesh(NavMesh.navMeshData);
            }
        }
    }

    private void DrawTiles()
    {
        if (currentTile == null) return;
        CircleDraw(currentTile);
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

    private void CircleDraw(SO_Tiles pRuleTile)
    {
        if (tilemaps == null || tilemaps.Count == 0 || camera == null) return;

        Vector3 mouse = Input.mousePosition;
        mouse.z = -camera.transform.position.z;
        Vector3 worldPos = camera.ScreenToWorldPoint(mouse);
        Vector3Int midCell = tilemaps[0].WorldToCell(new Vector3(worldPos.x, worldPos.y, 0f));

        int size = Mathf.Max(0, (int)BrushSizeSlider.value);
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

                    if (pRuleTile == waterTile)
                    {
                        ReplaceByWater(cellPos);
                    }
                    else
                    {
                        if (target == null)
                            target = FindTargetTilemap(cellPos, pRuleTile);
                        if (target == null) continue;

                        target.SetTile(cellPos, pRuleTile != null ? pRuleTile.RuleTiles : null);
                        if (pRuleTile != null)
                            target.SetColor(cellPos, colorBlender.BlendColorForTile());
                        else
                            target.SetColor(cellPos, Color.clear);
                    }
                }
            }
        }
    }

    private void ReplaceByWater(Vector3Int pCellPos)
    {
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap != waterTileMap)
            {
                tilemap.SetTile(pCellPos, null);
                tilemap.SetColor(pCellPos, Color.clear);
            }
        }
    }
}
