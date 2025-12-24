using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Brush
{
    public class TileMapManager : MonoBehaviour
    {
        [SerializeField] private List<Tilemap> tilemaps;
        private SO_Tiles currentTile;

        
        public static TileMapManager Instance;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public Tilemap FindTilemap(SO_Tiles _Tile)
        {
            if(_Tile == null || tilemaps == null) {return null;}

            foreach (Tilemap _tilemap in tilemaps)
            {
                if ((_Tile.LayerMask & (1 << _tilemap.gameObject.layer)) != 0)
                {
                    return _tilemap;
                }
            }
            return null;
        }


        public TileHandler GetTile()
        {
            if (currentTile != null)
            {
                TileHandler _tile = new TileHandler(FindTilemap(currentTile), currentTile);
                return _tile;
            }
            return new TileHandler();
        }

        public void SetTileButton(SO_Tiles _Tile)
        {
            currentTile =  _Tile;
            ColorBlender.Instance.SetColorForTile(_Tile);
        }
    }

    public struct TileHandler
    {
        public Tilemap Tilemap;
        public SO_Tiles Tile;

        public TileHandler(Tilemap _Tilemap, SO_Tiles _Tile)
        {
            Tilemap = _Tilemap;
            Tile = _Tile;
        }
    }
}
