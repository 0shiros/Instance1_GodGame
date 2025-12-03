using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class TileBrush : MonoBehaviour
{
    [SerializeField] new Camera camera;
    [SerializeField] List<SO_Tiles> tiles;
    [SerializeField] List<Tilemap> tilemaps;
    [SerializeField] int radius;
    [SerializeField] int minRadius;
    [SerializeField] int maxRadius;
    bool canDraw;
    bool canErase;
    int currentTileindex;


    private void Update()
    {
        if (canDraw)
        {
            DrawTiles();
        }

        if (canErase)
        {
            EraseTiles();
        }
        
        if (Input.GetKeyDown("up"))
        {
            currentTileindex++;
            currentTileindex = Mathf.Clamp(currentTileindex, 0, tiles.Count-1);
            Debug.Log(currentTileindex);
        }

        if (Input.GetKeyDown("down"))
        {
            currentTileindex--;
            currentTileindex = Mathf.Clamp(currentTileindex, 0, tiles.Count - 1);
            Debug.Log(currentTileindex);
        }
    }

    public void CanDraw(InputAction.CallbackContext context)
    {
        if (context.started) 
        { 
            canDraw = true;
        }

        if (context.canceled)
        {
            canDraw = false;
        }
    }
    
    public void CanErase(InputAction.CallbackContext context)
    {
        if (context.started) 
        { 
            canErase = true;
        }

        if (context.canceled)
        {
            canErase = false;
        }
    }

    public void ModifyRadius(InputAction.CallbackContext context)
    {
        if (context.ReadValue<int>() < 0 && radius > minRadius)
        {
            radius-=2;
        }
        if (context.ReadValue<int>() > 0 && radius < maxRadius)
        {
            radius+=2;
        }
    }

    private void DrawTiles()
    {
        for (int i = currentTileindex; i >= 0; i--)
        {
            CircleDraw(tiles[i].RuleTiles);
        }
    }
    
    private void EraseTiles()
    {
        for (int i = tilemaps.Count - 1; i >= 0; i--)
        {
            CircleDraw(null);
        }
    }

    private void CircleDraw(RuleTile pRuleTile, bool pIsCircle = false)
    {
        Vector3 pos = camera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int MidCell = tilemaps[0].WorldToCell(new Vector3(pos.x, pos.y));
        List<Vector3Int> cirlePositions = new List<Vector3Int>();
        int x = 0;
        int y = radius;
        int p = 1 - radius;

        while (x < y)
        {
            if (p < 0)
            {
                p += 2 * x + 1;
            }
            else
            {
                y--;
                p += 2 * (x + y) + 1;
            }
            
            cirlePositions.Add(new Vector3Int(MidCell.x + x, MidCell.y + y));
            cirlePositions.Add(new Vector3Int(MidCell.x - x, MidCell.y + y));
            cirlePositions.Add(new Vector3Int(MidCell.x + x, MidCell.y - y));
            cirlePositions.Add(new Vector3Int(MidCell.x - x, MidCell.y - y));
            cirlePositions.Add(new Vector3Int(MidCell.x + y, MidCell.y + x));
            cirlePositions.Add(new Vector3Int(MidCell.x - y, MidCell.y + x));
            cirlePositions.Add(new Vector3Int(MidCell.x + y, MidCell.y - x));
            cirlePositions.Add(new Vector3Int(MidCell.x - y, MidCell.y - x));

            #region dessine le cercle
            
            tilemaps[0].SetTile(new Vector3Int(MidCell.x + x, MidCell.y + y), pRuleTile);
            tilemaps[0].SetTile(new Vector3Int(MidCell.x - x, MidCell.y + y), pRuleTile);
            tilemaps[0].SetTile(new Vector3Int(MidCell.x + x, MidCell.y - y), pRuleTile);
            tilemaps[0].SetTile(new Vector3Int(MidCell.x - x, MidCell.y - y), pRuleTile);
            tilemaps[0].SetTile(new Vector3Int(MidCell.x + y, MidCell.y + x), pRuleTile);
            tilemaps[0].SetTile(new Vector3Int(MidCell.x - y, MidCell.y + x), pRuleTile);
            tilemaps[0].SetTile(new Vector3Int(MidCell.x + y, MidCell.y - x), pRuleTile);
            tilemaps[0].SetTile(new Vector3Int(MidCell.x - y, MidCell.y - x), pRuleTile);

            #endregion
            
            x++;
        }
        // DrawDisc(cirlePositions, MidCell, ruleTile);
    }

    // private void DrawDisc(List<Vector3Int> cirlePositions,Vector3Int MidCell, RuleTile ruleTile)
    // {
    //     foreach (Vector3Int position in cirlePositions)
    //     {
    //         if (position.x > MidCell.x && position.y > MidCell.y)
    //         {
    //             for (int i = 0; i < radius - 1; i++)
    //             {
    //                 tilemaps[0].SetTile(new Vector3Int(position.x - i, position.y), ruleTile);
    //                 tilemaps[0].SetTile(new Vector3Int(position.x, position.y - i), ruleTile);
    //             }
    //         }
    //         else if (position.x < MidCell.x && position.y > MidCell.y)
    //         {
    //             for (int i = 0; i < radius - 1; i++)
    //             {
    //                 tilemaps[0].SetTile(new Vector3Int(position.x + i, position.y), ruleTile);
    //                 tilemaps[0].SetTile(new Vector3Int(position.x, position.y - i), ruleTile);
    //             }
    //         }
    //         else if (position.x > MidCell.x && position.y < MidCell.y)
    //         {
    //             for (int i = 0; i < radius - 1; i++)
    //             {
    //                 tilemaps[0].SetTile(new Vector3Int(position.x - i, position.y), ruleTile);
    //                 tilemaps[0].SetTile(new Vector3Int(position.x, position.y + i), ruleTile);
    //             }
    //         }
    //         else if(position.x < MidCell.x && position.y < MidCell.y)
    //         {
    //             for (int i = 0; i < radius - 1; i++)
    //             {
    //                 tilemaps[0].SetTile(new Vector3Int(position.x + i, position.y), ruleTile);
    //                 tilemaps[0].SetTile(new Vector3Int(position.x, position.y + i), ruleTile);
    //             }
    //         }
    //     }
    //     int startPos = (radius%2 == 0) ? radius/2 : radius-1/2;
    //     Vector3Int startPosVector =  MidCell;
    //     for (int y = 0; y < radius - 2; y++)
    //     {
    //         for (int x = 0; x < radius - 2; x++)
    //         {
    //             tilemaps[0].SetTile(new Vector3Int(startPosVector.x - x, startPosVector.y - y), ruleTile);
    //         }
    //     }
    // }

    /*private void LineDraw(Vector3Int pos, Vector3Int target, RuleTile ruleTile)
    {
        int dx = target.x - pos.x;
        int dy = target.y - pos.y;

        if (dx != 0)
        {
            int y = pos.y;
            int p = 2 * dy - dx;
            for (int i = 0; i < dx+1; i++)
            {
                tilemaps[0].SetTile(new Vector3Int(pos.x, Mathf.RoundToInt(pos.y + i), y), ruleTile);
                
                if (p >= 0)
                {
                    y++;
                    p -= 2 * dx;
                }
                p += 2 * dy;
            }
        }
    }*/
}
