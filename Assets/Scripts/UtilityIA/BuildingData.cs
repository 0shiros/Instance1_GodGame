// BuildingData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingData", menuName = "CityAI/BuildingData")]
public class BuildingData : ScriptableObject
{
    public string BuildingName = "New Building";
    public GameObject Prefab;
    public Vector2Int Size = Vector2Int.one; 
    public int WoodCost = 5;
    public int StoneCost = 3;
    public int MetalCost = 4;
    public int HousingCapacity = 2;
    public BuildingType BuildingType = BuildingType.None;
    public bool isStorage = false; 
    [Header("Placement par case")]
    [Tooltip("Combien de bâtiments maximum peuvent être placés dans une même case.")]
    public int maxBuildingsPerCell = 3;

    [Tooltip("Rayon (en unité locale) pour randomiser la position à l'intérieur de la case.")]
    public float placementOffsetRadius = 0.35f;
}
