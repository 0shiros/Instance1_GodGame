// TaskData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewTaskData", menuName = "CityAI/TaskData")]
public class TaskData : ScriptableObject
{
    [Header("Général")]
    public string taskName = "New Task";
    public TaskType type = TaskType.Collect;

    [Header("Paramètres Collecte")]
    public ResourceType targetResource = ResourceType.None; // pour Collect

    [Header("Paramètres Construction")]
    public BuildingType targetBuildingType = BuildingType.None; // pour Build

    [Header("Priorité")]
    public float basePriority = 1f;           // priorité de base
    public int recommendedVillagers = 1;      // nb conseillé
}
