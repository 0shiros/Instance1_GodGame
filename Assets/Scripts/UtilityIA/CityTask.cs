using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CityTask
{
    public TaskData Data;


    public ResourceNode ResourceTarget;


    public BuildingData BuildingData;
    public Vector3 BuildPosition;

    public List<VillagerUtilityAI> AassignedVillagers = new List<VillagerUtilityAI>();

    public bool IsCompleted = false;

    public void MarkCompleted()
    {
        IsCompleted = true;
    }
}
