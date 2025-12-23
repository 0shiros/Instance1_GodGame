using System;
using System.Collections.Generic;
using Brush;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.Events;
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
    [SerializeField] private SO_MapData mapBounds;
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
    private BrushPreview previewBrush;
    private Vector3Int oldMidCell;

    public UnityEvent<int> OnQuestComplete = new UnityEvent<int>();

    private void OnDisable()
    {
        NavMesh.RemoveData();
    }

    public SO_Tiles GetTile()
    {
        return currentTile;
    }

    private void Start()
    {
        colorBlender = ColorBlender.Instance;
        previewBrush = GetComponent<BrushPreview>();
        BrushSizeSlider.onValueChanged.AddListener(SizeChanged);
        SetWaterTile();
    }

    private void SetWaterTile()
    {
        for (int x = Mathf.CeilToInt((mapBounds.MapBounds.x / 2) * -1);
             x < Mathf.CeilToInt(mapBounds.MapBounds.x / 2);
             x++)
        {
            for (int y = Mathf.CeilToInt((mapBounds.MapBounds.y / 2) * -1);
                 y < Mathf.CeilToInt(mapBounds.MapBounds.x / 2);
                 y++)
            {
                waterTileMap.SetTile(new Vector3Int(x, y, 0), waterTile.RuleTiles);
                waterTileMap.SetColor(new Vector3Int(x, y, 0), waterTile.Color);
            }
        }
    }

    public void Reset()
    {
        currentTile = null;
        previewBrush.HidePreview();
    }

    private void SizeChanged(float pArg0)
    {
        OnQuestComplete.Invoke(1);
        previewBrush.SetSize((int)pArg0);
    }

    public void GetTile(SO_Tiles pTile)
    {
        currentTile = pTile;
        colorBlender.SetColorForTile(pTile);
        previewBrush.ShowPreview();
        target = null;


        switch (pTile.TileType)
        {
            case ETileType.Water:
                break;
            case ETileType.Dirt:
                break;
            case ETileType.Grass:
                OnQuestComplete.Invoke(2);
                break;
            case ETileType.Sand:
                OnQuestComplete.Invoke(0);
                break;
            case ETileType.HeightGrass:
                break;
        }
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
        if (tilemaps == null || tilemaps.Count == 0 || camera == null) return;

        Vector3Int _midCell = GetMidCell();

        oldMidCell = _midCell;

        int _size = Mathf.Max(0, (int)BrushSizeSlider.value);

        if (currentTile != EraseTile)
        {
            target = TileMapManager.Instance.FindTilemap(currentTile);
        }

        List<Vector3Int> _cells = Shaper.Instance.SquareShape(_size, _midCell);

        foreach (Vector3Int _cellPos in _cells)
        {
            if (currentTile == waterTile)
            {
                ReplaceByWater(_cellPos);
            }
            else
            {
                if (target == null) continue;

                target.SetTile(_cellPos, currentTile != null ? currentTile.RuleTiles : null);
                if (currentTile != null)
                    target.SetColor(_cellPos, colorBlender.BlendColorForTile());
                else
                    target.SetColor(_cellPos, Color.clear);
            }
        }
    }


    private Vector3Int GetMidCell()
    {
        Vector3 _mouse = Input.mousePosition;
        _mouse.z = -camera.transform.position.z;
        Vector3 _worldPos = camera.ScreenToWorldPoint(_mouse);
        Vector3Int _midCell = waterTileMap.WorldToCell(new Vector3(_worldPos.x, _worldPos.y, 0f));
        return _midCell;
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

    public void ClearCurrentTileGround()
    {
        currentTile = null;
    }
}