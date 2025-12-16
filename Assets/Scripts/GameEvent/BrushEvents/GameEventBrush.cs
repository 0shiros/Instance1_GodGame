using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameEventBrush : MonoBehaviour
{
    [SerializeField] int offset;
    [SerializeField] SO_Tiles EraseTile;
    private Tilemap target;
    
    private Tilemap FindTargetTilemap(Vector3Int pos, SO_Tiles pRuleTile, List<Tilemap> tilemaps)
    {
        if (tilemaps == null || tilemaps.Count == 0) return null;

        if (pRuleTile != null && pRuleTile.LayerMask != 0)
        {
            for (int i = tilemaps.Count - 1; i >= 0; i--)
            {
                var tm = tilemaps[i];
                if (tm == null) continue;
                if ((pRuleTile.LayerMask & (1 << tm.gameObject.layer)) != 0)
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
    public IEnumerator CircleDraw(SO_Tiles pRuleTile, Vector3Int pMidCell, int pRadius, List<Tilemap> tilemaps, float pDelay = 0)
    {
        int tempOffSet = offset;
        
        foreach (Tilemap tilemap in tilemaps)
        {
            Debug.Log("Drawing circle at "+pMidCell);
            if (tilemaps == null || tilemaps.Count == 0) break;

            int size = Mathf.Max(0, pRadius);
            int rSq = size * size;

            if (pRuleTile == EraseTile && target == null)
            {
                target = FindTargetTilemap(pMidCell, pRuleTile, tilemaps);
            }

            for (int dx = -size; dx <= size; dx++)
            {
                for (int dy = -size; dy <= size; dy++)
                {
                    if (dx * dx + dy * dy <= rSq)
                    {
                        Vector3Int cellPos = new Vector3Int(pMidCell.x + dx, pMidCell.y + dy, pMidCell.z);

                        if (target == null)
                            target = FindTargetTilemap(cellPos, pRuleTile, tilemaps);
                        if (target == null) continue;

                        tilemap.SetTile(cellPos, null);
                        tilemap.SetColor(cellPos, Color.clear);
                    }
                }
            }
            pRadius -= tempOffSet;
            tempOffSet++;
            yield return new WaitForSeconds(pDelay);
        }
    }
}
