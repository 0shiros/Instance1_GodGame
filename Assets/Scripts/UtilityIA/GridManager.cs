#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class GridManager2D : MonoBehaviour
{
    public int Width = 10;
    public int Height = 10;
    public float CellSize = 1f;

    private ITileOccupant[,] grid;
    private bool[,] visibleCells; // indique si la case est visible (contient un bâtiment)

    void Awake()
    {
        grid = new ITileOccupant[Width, Height];
        visibleCells = new bool[Width, Height];
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

    public bool CanPlaceAt(int x, int y, Vector2Int size)
    {
        if (x < 0 || y < 0 || x + size.x > Width || y + size.y > Height)
            return false;

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                var occ = grid[x + i, y + j] as MonoBehaviour;
                if (occ == null) continue;
                if (occ.CompareTag("Resource")) continue;
                return false;
            }
        }
        return true;
    }

    public bool Place(ITileOccupant occ, int x, int y, Vector2Int size)
    {
        if (!CanPlaceAt(x, y, size)) return false;

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                grid[x + i, y + j] = occ;

                // si c'est un bâtiment, rendre visible la case
                if (occ != null && !((occ as MonoBehaviour)?.CompareTag("Resource") ?? true))
                    visibleCells[x + i, y + j] = true;
            }
        }
        return true;
    }

    public void Remove(ITileOccupant occ, int x, int y, Vector2Int size)
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                grid[x + i, y + j] = null;
                visibleCells[x + i, y + j] = false; // supprime visibilité si case vide
            }
        }
    }

    // Dessine la grille et les cases visibles en 2D
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        // lignes de la grille
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

        // cases visibles en vert
        if (grid != null)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (visibleCells[x, y])
                    {
                        Vector3 pos = CellToWorld(x, y) + new Vector3(CellSize / 2f, CellSize / 2f, 0);
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireCube(pos, new Vector3(CellSize, CellSize, 0.01f));
                    }
                }
            }
        }
    }

    // Vérifie si une cellule est visible (pour les bâtiments)
    public bool IsCellVisible(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return false;
        return visibleCells[x, y];
    }
}
