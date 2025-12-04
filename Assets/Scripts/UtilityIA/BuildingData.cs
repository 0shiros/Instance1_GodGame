// BuildingData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingData", menuName = "CityAI/BuildingData")]
public class BuildingData : ScriptableObject
{
    public string buildingName = "New Building";
    public GameObject prefab;
    public Vector2Int size = Vector2Int.one; // utilisé pour OverlapBox (grid)
    public int woodCost = 5;
    public int stoneCost = 3;
    public int housingCapacity = 2;
    public BuildingType buildingType = BuildingType.None;
    public bool isStorage = false; // si le bâtiment peut stocker/déposer
}
