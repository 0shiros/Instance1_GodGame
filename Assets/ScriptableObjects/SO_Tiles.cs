using System.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Tiles", menuName = "Scriptable Objects/SO_Tiles")]
public class SO_Tiles : ScriptableObject
{
    public RuleTile RuleTiles;
    public Color color;
    public LayerMask layerMask;
}
