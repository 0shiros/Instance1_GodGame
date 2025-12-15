using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class GridManager2D : MonoBehaviour
{
    public int Width = 10;
    public int Height = 10;
    public float CellSize = 1f;

    
    public class GridCell
    {
        public CityUtilityAI ownerCityAI = null;
        public List<GameObject> placedBuildings = new List<GameObject>();
        public int reservedCount = 0;
    }

    private GridCell[,] grid;


    private GameObject[,] cellVisuals;

    void Awake()
    {
        InitGrid();
        
    }

    void OnValidate()
    {
        if (Width < 1) Width = 1;
        if (Height < 1) Height = 1;
        if (CellSize <= 0f) CellSize = 1f;
        InitGrid();
    }

    private void InitGrid()
    {
        grid = new GridCell[Width, Height];
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                if (grid[x, y] == null)
                    grid[x, y] = new GridCell();


        if (cellVisuals != null)
        {
            for (int x = 0; x < cellVisuals.GetLength(0); x++)
                for (int y = 0; y < cellVisuals.GetLength(1); y++)
                    if (cellVisuals[x, y] != null)
                        DestroyImmediate(cellVisuals[x, y]);
        }
        cellVisuals = new GameObject[Width, Height];
    }

    #region Conversion / utilitaires

    public Vector3 CellToWorld(int x, int y)
    {
        return transform.position + new Vector3(x * CellSize, y * CellSize, 0f);
    }

    public Vector2Int WorldToCell(Vector3 world)
    {
        Vector3 local = world - transform.position;
        int x = Mathf.FloorToInt(local.x / CellSize);
        int y = Mathf.FloorToInt(local.y / CellSize);
        return new Vector2Int(x, y);
    }

    public bool IsValidCell(Vector2Int c) => c.x >= 0 && c.y >= 0 && c.x < Width && c.y < Height;

    public Vector3 RandomPosInsideCell(Vector2Int cell, float offsetRadius)
    {
        Vector3 center = CellToWorld(cell.x, cell.y) + new Vector3(CellSize / 2f, CellSize / 2f, 0f);
        float ox = Random.Range(-offsetRadius, offsetRadius);
        float oy = Random.Range(-offsetRadius, offsetRadius);
        return center + new Vector3(ox, oy, 0f);
    }

    #endregion

    #region Query / placement checks

    public bool CanPlaceAt(int x, int y, Vector2Int size, int maxPerCell = 1)
    {
        if (x < 0 || y < 0 || x + size.x > Width || y + size.y > Height) return false;

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                var c = grid[x + i, y + j];
                if (c == null) return false;
                int occupancy = c.placedBuildings.Count + c.reservedCount;
                if (occupancy >= maxPerCell) return false;
            }
        }
        return true;
    }

    #endregion

    #region Reservation / commit

    public bool TryReserveCell(CityUtilityAI owner, Vector2Int cell, BuildingData buildingData, out Vector3 reservedWorldPos)
    {
       
        reservedWorldPos = Vector3.zero;
        if (!IsValidCell(cell)) return false;

        int maxPerCell = 1;
        float offsetRadius = CellSize * 0.4f;

        if (!CanPlaceAt(cell.x, cell.y, buildingData != null ? buildingData.Size : Vector2Int.one, maxPerCell))
            return false;

        var c = grid[cell.x, cell.y];
        if (c.ownerCityAI != null && c.ownerCityAI != owner) return false;

        if (c.ownerCityAI == null) c.ownerCityAI = owner;
        c.reservedCount++;

        reservedWorldPos = RandomPosInsideCell(cell, offsetRadius);
        return true;

    }

    public void ReleaseReservation(Vector2Int cell)
    {
        if (!IsValidCell(cell)) return;
        var c = grid[cell.x, cell.y];
        c.reservedCount = Mathf.Max(0, c.reservedCount - 1);
        Debug.Log(c.ownerCityAI);
    }

    public bool CommitPlacement(Vector2Int cell, GameObject placed)
    {
        if (!IsValidCell(cell)) return false;
        var c = grid[cell.x, cell.y];

        c.placedBuildings.Add(placed);
        if (c.reservedCount > 0) c.reservedCount--;
        

        UpdateCellVisual(cell);

        return true;
    }

    #endregion

    #region Recherches de cellules

    public List<Vector2Int> GetCellsOwnedByCity(CityUtilityAI owner)
    {
        List<Vector2Int> res = new List<Vector2Int>();
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                var c = grid[x, y];
                if (c != null && c.ownerCityAI == owner)
                    res.Add(new Vector2Int(x, y));
            }
        return res;
    }

    public bool TryFindNearestFreeCell(Vector3 origin, Vector2Int size, out Vector2Int result)
    {
        result = Vector2Int.zero;
        float bestDist = float.MaxValue;
        bool found = false;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (!CanPlaceAt(x, y, size)) continue;
                Vector3 cellWorld = CellToWorld(x, y) + new Vector3(CellSize / 2f, CellSize / 2f, 0f);
                float d = Vector3.Distance(origin, cellWorld);
                if (d < bestDist)
                {
                    bestDist = d;
                    result = new Vector2Int(x, y);
                    found = true;
                }
            }
        }
        return found;
    }

    public bool TryFindCellAdjacentToCity(CityUtilityAI owner, BuildingData buildingData, out Vector2Int found, int maxRadius = 3)
    {
        found = Vector2Int.zero;
        if (owner == null) return false;

        var owned = GetCellsOwnedByCity(owner);
        if (owned == null || owned.Count == 0) return false;
        owned = owned.OrderBy(x => Random.value).ToList();

        for (int r = 1; r <= maxRadius; r++)
        {
            foreach (var baseCell in owned)
            {
                List<Vector2Int> neighbours = new List<Vector2Int>();
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        if (Mathf.Abs(dx) + Mathf.Abs(dy) != 1) continue;
                        Vector2Int candidate = new Vector2Int(baseCell.x + dx, baseCell.y + dy);
                        if (IsValidCell(candidate))
                            neighbours.Add(candidate);
                    }
                }
                neighbours = neighbours.OrderBy(x => Random.value).ToList();
                foreach (var candidate in neighbours)
                {
                    if (!CanPlaceAt(candidate.x, candidate.y, buildingData != null ? buildingData.Size : Vector2Int.one))
                        continue;

                    var c = grid[candidate.x, candidate.y];
                    if (c.ownerCityAI != null && c.ownerCityAI != owner) continue;

                    found = candidate;
                    return true;
                }
            }
        }
        return false;
    }

    public bool TryFindRandomCellAroundHouse(Vector2Int size, out Vector2Int foundCell, int tries = 200)
    {
        foundCell = Vector2Int.zero;
        for (int i = 0; i < tries; i++)
        {
            int x = Random.Range(0, Width);
            int y = Random.Range(0, Height);
            if (CanPlaceAt(x, y, size))
            {
                foundCell = new Vector2Int(x, y);
                return true;
            }
        }
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                if (CanPlaceAt(x, y, size))
                {
                    foundCell = new Vector2Int(x, y);
                    return true;
                }
        return false;
    }

    #endregion
   
    #region Visualisation runtime pour les maisons

    private void UpdateCellVisual(Vector2Int cell)
    {
        var c = grid[cell.x, cell.y];
        bool hasHouse = c.placedBuildings.Any(b => b != null && b.CompareTag("Maison"));
        
        if (hasHouse)
        {
            if (cellVisuals[cell.x, cell.y] == null)
            {
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.position = CellToWorld(cell.x, cell.y) + new Vector3(CellSize / 2f, CellSize / 2f, -0.01f);
                quad.transform.localScale = new Vector3(CellSize, CellSize, 1f);
                quad.GetComponent<MeshRenderer>().material.color = Color.green;
                quad.name = $"CellVisual_{cell.x}_{cell.y}";
                quad.transform.parent = transform;
                cellVisuals[cell.x, cell.y] = quad;
            }
        }
        else
        {
            if (cellVisuals[cell.x, cell.y] != null)
            {
                DestroyImmediate(cellVisuals[cell.x, cell.y]);
                cellVisuals[cell.x, cell.y] = null;
            }
        }
    }

    #endregion

    #region Gizmos / visuel éditeur

    private void OnDrawGizmos()
    {
        if (grid == null || grid.GetLength(0) != Width || grid.GetLength(1) != Height)
            InitGrid();

        Gizmos.color = Color.gray;
        for (int x = 0; x <= Width; x++)
        {
            Vector3 from = transform.position + new Vector3(x * CellSize, 0, 0);
            Vector3 to = from + new Vector3(0, Height * CellSize, 0);
            Gizmos.DrawLine(from, to);
        }
        for (int y = 0; y <= Height; y++)
        {
            Vector3 from = transform.position + new Vector3(0, y * CellSize, 0);
            Vector3 to = from + new Vector3(Width * CellSize, 0, 0);
            Gizmos.DrawLine(from, to);
        }


        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var c = grid[x, y];
                if (c == null) continue;
                if (c.ownerCityAI != null)
                {
                    Vector3 pos = CellToWorld(x, y) + new Vector3(CellSize / 2f, CellSize / 2f, 0);
                    Gizmos.color = Color.Lerp(Color.blue, Color.cyan, 0.5f);
                    Gizmos.DrawCube(pos, new Vector3(CellSize * 0.9f, CellSize * 0.9f, 0.01f));
                }
                else if (c.placedBuildings.Count > 0)
                {
                    Vector3 pos = CellToWorld(x, y) + new Vector3(CellSize / 2f, CellSize / 2f, 0);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(pos, new Vector3(CellSize * 0.9f, CellSize * 0.9f, 0.01f));
                }
                else if (c.reservedCount > 0)
                {
                    Vector3 pos = CellToWorld(x, y) + new Vector3(CellSize / 2f, CellSize / 2f, 0);
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube(pos, new Vector3(CellSize * 0.7f, CellSize * 0.7f, 0.01f));
                }
            }
        }
    }

    #endregion
}
