using UnityEngine;

public class GridManager2D : MonoBehaviour
{
    public int Width = 10;
    public int Height = 10;
    public float CellSize = 1f;

    private ITileOccupant[,] grid;
    private bool[,] visibleCells;

    //  propriétaire des cases
    private CityUtilityAI[,] ownerCells;

    void Awake()
    {
        grid = new ITileOccupant[Width, Height];
        visibleCells = new bool[Width, Height];
        ownerCells = new CityUtilityAI[Width, Height];
    }

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

    //  vérifier si la cellule appartient déjà à un autre CityAI
    private bool CellOwnedByAnotherCity(int x, int y, CityUtilityAI requester)
    {
        var owner = ownerCells[x, y];
        return owner != null && owner != requester;
    }

    // ★ MODIFIÉ : maintenant CanPlaceAt vérifie l'appartenance des cases
    public bool CanPlaceAt(int x, int y, Vector2Int size, CityUtilityAI requester)
    {
        if (x < 0 || y < 0 || x + size.x > Width || y + size.y > Height)
            return false;

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (CellOwnedByAnotherCity(x + i, y + j, requester))
                    return false; // ★ interdit de construire sur une zone ennemie

                if (grid[x + i, y + j] != null)
                {
                    var occ = grid[x + i, y + j] as MonoBehaviour;

                    if (occ == null) continue;
                    if (occ.CompareTag("Resource")) continue;

                    return false;
                }
            }
        }
        return true;
    }

    // ★ MODIFIÉ : Place demande maintenant le CityAI qui construit
    public bool Place(ITileOccupant occ, int x, int y, Vector2Int size, CityUtilityAI ownerCity)
    {
        if (!CanPlaceAt(x, y, size, ownerCity)) return false;

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                grid[x + i, y + j] = occ;

                // visible
                if (occ != null && !((occ as MonoBehaviour)?.CompareTag("Resource") ?? true))
                    visibleCells[x + i, y + j] = true;

                //  on attribue la case au CityAI
                ownerCells[x + i, y + j] = ownerCity;
            }
        }
        return true;
    }

    //  réserve une zone entière
    public void ClaimArea(int x, int y, Vector2Int size, CityUtilityAI ownerCity)
    {
        for (int i = 0; i < size.x; i++)
            for (int j = 0; j < size.y; j++)
                ownerCells[x + i, y + j] = ownerCity;
    }

    public void Remove(ITileOccupant occ, int x, int y, Vector2Int size)
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                grid[x + i, y + j] = null;
                visibleCells[x + i, y + j] = false;
                ownerCells[x + i, y + j] = null;
            }
        }
    }

    private void OnDrawGizmos()
    {
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

        if (visibleCells != null)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (visibleCells[x, y])
                    {
                        Vector3 pos = CellToWorld(x, y) + new Vector3(CellSize / 2f, CellSize / 2f, 0);
                        Gizmos.color = Color.green;
                        Gizmos.DrawCube(pos, new Vector3(CellSize, CellSize, 0.1f));
                    }
                }
            }
        }

        //  debug des zones appartenant à un CityAI
        if (ownerCells != null)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (ownerCells[x, y] != null)
                    {
                        Vector3 pos = CellToWorld(x, y) + new Vector3(CellSize / 2f, CellSize / 2f, 0);
                        Gizmos.color = Color.blue * 0.5f;
                        Gizmos.DrawWireCube(pos, new Vector3(CellSize, CellSize, 0.05f));
                    }
                }
            }
        }
    }

    public bool IsCellVisible(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return false;
        return visibleCells[x, y];
    }

    public CityUtilityAI GetOwner(int x, int y)
    {
        return ownerCells[x, y];
    }
}
