using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class EnvironementBrush : MonoBehaviour
{
    [SerializeField] private CustomTile currentTile;
    [SerializeField] Camera camera;
    [SerializeField] SO_Tiles eraseTile;
    [SerializeField] List<Tilemap> tilemaps;
    [SerializeField] Tilemap target;
    [SerializeField] private NavMeshSurface navMesh;
    bool canDraw;
    private bool isSelected;

    ColorBlender colorBlender;

    void Start()
    {
        colorBlender = ColorBlender.Instance;
    }

    public void SetTile(CustomTile pTile)
    {
        currentTile = pTile;
        colorBlender.SetColorForCustomTile(pTile);
    }

    public void TileSelected(bool pIsSelected)
    {
        isSelected = pIsSelected;
    }

    private void Update()
    {
        if (IsPointerOverUI()) return;

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
        if (context.started && HasTileGroundAtPosition()) canDraw = true;
        if (context.canceled)
        {
            canDraw = false;
            if (navMesh != null)
            {
                navMesh.UpdateNavMesh(navMesh.navMeshData);
            }
        }
    }
    
    private bool HasTileGroundAtPosition()
    {
        Vector3Int pPosition = target.WorldToCell(camera.ScreenToWorldPoint(Input.mousePosition));
        
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.GetTile(pPosition) != null)
            {
                return true;
            }
        }
        return false;
    }

    private void DrawTiles()
    {
        if (currentTile == null) return;
        CircleDraw(currentTile);
    }

    private void CircleDraw(CustomTile pRuleTile)
    {
        if (target == null || camera == null) return;

        Vector3 mouse = Input.mousePosition;
        mouse.z = -camera.transform.position.z;
        Vector3 worldPos = camera.ScreenToWorldPoint(mouse);
        Vector3Int midCell = target.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0f));
        
        for (int i = 0; i < pRuleTile.Sources.Count; i++)
        {
            switch (pRuleTile.Sources[i].Direction)
            {
                case ETileDirection.Top:
                    target.SetTile(new Vector3Int(midCell.x, midCell.y + 1, 0), pRuleTile.Sources[i].Sprites);
                    target.SetColor(new Vector3Int(midCell.x, midCell.y + 1, 0), colorBlender.BlendColorForCustomTile(i));
                    break;
                case ETileDirection.Bottom:
                    target.SetTile(new Vector3Int(midCell.x, midCell.y - 1, 0), pRuleTile.Sources[i].Sprites);
                    target.SetColor(new Vector3Int(midCell.x, midCell.y - 1, 0), colorBlender.BlendColorForCustomTile(i));
                    break;
                case ETileDirection.Left:
                    target.SetTile(new Vector3Int(midCell.x - 1, midCell.y, 0), pRuleTile.Sources[i].Sprites);
                    target.SetColor(new Vector3Int(midCell.x - 1, midCell.y, 0), colorBlender.BlendColorForCustomTile(i));
                    break;
                case ETileDirection.Right:
                    target.SetTile(new Vector3Int(midCell.x + 1, midCell.y + 1, 0), pRuleTile.Sources[i].Sprites);
                    target.SetColor(new Vector3Int(midCell.x + 1, midCell.y, 0), colorBlender.BlendColorForCustomTile(i));
                    break;
                default:
                    target.SetTile(new Vector3Int(midCell.x, midCell.y, 0), pRuleTile.Sources[i].Sprites);
                    target.SetColor(new Vector3Int(midCell.x, midCell.y, 0), colorBlender.BlendColorForCustomTile(i));
                    break;
            }
        }
    }
    
    public void ClearCurrentTileEnvironement()
    {
        if(currentTile == null) return;
        currentTile = null;
    }
}