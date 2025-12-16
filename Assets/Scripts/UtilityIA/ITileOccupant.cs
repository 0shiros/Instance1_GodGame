using UnityEngine;

public interface ITileOccupant
{
    Vector2Int GetGridPosition();
    Vector2Int GetSize();
}
