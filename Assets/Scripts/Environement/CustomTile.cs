using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "CustomTile", menuName = "Scriptable Objects/CustomTile")]
public class CustomTile : ScriptableObject
{
    public List<CustomTileData> Sources = new List<CustomTileData>();
    public Sprite Sprite;
    [Header("not mandatory")]
    public int id;
    
    public string Name;
}


[Serializable]
public struct CustomTileData
{
    public Tile Sprites;
    public ETileDirection Direction;
    public Color Color;
    public string Name;
}

[Serializable]
public enum ETileDirection
{
    Top,
    Center,
    Bottom,
    Left,
    Right
}