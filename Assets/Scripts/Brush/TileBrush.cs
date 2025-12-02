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

    private void DrawTiles()
    {
        CircleDraw();
        /*Vector3 pos = camera.ScreenToWorldPoint(Input.mousePosition);
        for (int i = currentTileindex; i >= 0; i--)
        {
            Vector3Int cell = tilemaps[i].WorldToCell(new Vector3(pos.x, pos.y));
            tilemaps[i].SetTile(cell, tiles[i].RuleTiles);
        }*/
    }
    

    private void EraseTiles()
    {
        Vector3 pos = camera.ScreenToWorldPoint(Input.mousePosition);
        for (int i = tilemaps.Count - 1; i >= 0; i--)
        {
            Vector3Int cell = tilemaps[i].WorldToCell(new Vector3(pos.x, pos.y));
            tilemaps[i].SetTile(cell, null);
        }
    }

    private void CircleDraw()
    {
        Vector3 pos = camera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int MidCell = tilemaps[0].WorldToCell(new Vector3(pos.x, pos.y));
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
            
            tilemaps[0].SetTile(new Vector3Int(MidCell.x + x, MidCell.y + y), tiles[0].RuleTiles);
            tilemaps[0].SetTile(new Vector3Int(MidCell.x - x, MidCell.y + y), tiles[0].RuleTiles);
            tilemaps[0].SetTile(new Vector3Int(MidCell.x + x, MidCell.y - y), tiles[0].RuleTiles);
            tilemaps[0].SetTile(new Vector3Int(MidCell.x - x, MidCell.y - y), tiles[0].RuleTiles);
            tilemaps[0].SetTile(new Vector3Int(MidCell.x + y, MidCell.y + x), tiles[0].RuleTiles);
            tilemaps[0].SetTile(new Vector3Int(MidCell.x - y, MidCell.y + x), tiles[0].RuleTiles);
            tilemaps[0].SetTile(new Vector3Int(MidCell.x + y, MidCell.y - x), tiles[0].RuleTiles);
            tilemaps[0].SetTile(new Vector3Int(MidCell.x - y, MidCell.y - x), tiles[0].RuleTiles);
            x++;
        }
    }
}
