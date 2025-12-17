using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

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

    public void Reset()
    {
        currentTile = null;
    }

    public void SetTile(CustomTile pTile)
    {
        currentTile = pTile;
        colorBlender.SetColorForCustomTile(pTile);
    }

    public void TileSelected(bool pIsSelected)
    {
        isSelected = pIsSelected;
        if (isSelected)
        {
            GetComponent<BrushPreview>().HidePreview();
            Debug.Log("Selected");
        }
    }

    private void Update()
    {
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
        if (context.started && HasTileGroundAtPosition())
        {
            if (IsPointerOverUI()) return;
            DrawTiles();
        }

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
        if (!CanPlaceTile(pRuleTile, midCell))
            return;

        for (int i = 0; i < pRuleTile.Sources.Count; i++)
        {
            switch (pRuleTile.Sources[i].Direction)
            {
                case ETileDirection.Top:
                    target.SetTile(new Vector3Int(midCell.x, midCell.y + 1, 0), pRuleTile.Sources[i].Sprites);
                    target.SetColor(new Vector3Int(midCell.x, midCell.y + 1, 0),
                        colorBlender.BlendColorForCustomTile(i));
                    break;
                case ETileDirection.Bottom:
                    target.SetTile(new Vector3Int(midCell.x, midCell.y - 1, 0), pRuleTile.Sources[i].Sprites);
                    target.SetColor(new Vector3Int(midCell.x, midCell.y - 1, 0),
                        colorBlender.BlendColorForCustomTile(i));
                    break;
                case ETileDirection.Left:
                    target.SetTile(new Vector3Int(midCell.x - 1, midCell.y, 0), pRuleTile.Sources[i].Sprites);
                    target.SetColor(new Vector3Int(midCell.x - 1, midCell.y, 0),
                        colorBlender.BlendColorForCustomTile(i));
                    break;
                case ETileDirection.Right:
                    target.SetTile(new Vector3Int(midCell.x + 1, midCell.y + 1, 0), pRuleTile.Sources[i].Sprites);
                    target.SetColor(new Vector3Int(midCell.x + 1, midCell.y, 0),
                        colorBlender.BlendColorForCustomTile(i));
                    break;
                default:
                    target.SetTile(new Vector3Int(midCell.x, midCell.y, 0), pRuleTile.Sources[i].Sprites);
                    target.SetColor(new Vector3Int(midCell.x, midCell.y, 0), colorBlender.BlendColorForCustomTile(i));
                    break;
            }
        }

        switch (currentTile.id)
        {
            case 0: //tree quest
                Quest.Instance.CompleteQuest(3);
                break;
            case 1: //bush quest
                Quest.Instance.CompleteQuest(4);
                break;
            case 2: //stone quest
                Quest.Instance.CompleteQuest(5);
                break;
            case 3: //metal quest
                Quest.Instance.CompleteQuest(6);
                break;
            case 4: //townHall quest
                Quest.Instance.CompleteQuest(7);
                break;
        }
    }

    private bool CanPlaceTile(CustomTile pRuleTile, Vector3Int midCell)
    {
        for (int i = 0; i < pRuleTile.Sources.Count; i++)
        {
            Vector3Int checkPos = midCell;

            switch (pRuleTile.Sources[i].Direction)
            {
                case ETileDirection.Top:
                    checkPos += Vector3Int.up;
                    break;
                case ETileDirection.Bottom:
                    checkPos += Vector3Int.down;
                    break;
                case ETileDirection.Left:
                    checkPos += Vector3Int.left;
                    break;
                case ETileDirection.Right:
                    checkPos += Vector3Int.right;
                    break;
            }

            if (target.GetTile(checkPos) != null)
                return false;
        }

        return true;
    }

    public void ClearCurrentTileEnvironement()
    {
        if (currentTile == null) return;
        currentTile = null;
    }

    public CustomTile GetTile()
    {
        return currentTile;
    }
}