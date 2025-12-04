// CityTask.cs
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CityTask
{
    public TaskData data;
    public ResourceNode resourceTarget; // pour Collect
    public BuildingData buildingData;   // pour Build
    public Vector3 buildPosition;       // pour Build
    public StorageBuilding depositTarget; // pour Deposit (dépôt)
    public List<VillagerUtilityAI> assignedVillagers = new List<VillagerUtilityAI>();
    public bool isCompleted = false;
}
