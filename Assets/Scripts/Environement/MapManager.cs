using UnityEngine;
using UnityEngine.Tilemaps;

namespace Environement
{
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private SO_MapData mapBounds;
        [SerializeField] private Tilemap waterTileMap;
        [SerializeField] private SO_Tiles waterTile;

        void Start()
        {
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
    }
}
