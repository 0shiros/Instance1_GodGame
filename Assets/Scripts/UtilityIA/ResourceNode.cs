// ResourceNode.cs
using System;
using UnityEngine;
using UnityEngine.Audio;

public class ResourceNode : MonoBehaviour
{
    [Header("Ressource")]
    public ResourceType ResourceType = ResourceType.Wood;
    public int Amount = int.MaxValue;              
    public int HarvestPerAction = 1;
    public static Action<ResourceNode> ActionResource;
    private void Start()
    {
        AddSciencePoints();
    }
    private void AddSciencePoints()
    {
        ActionResource?.Invoke(this);
    }
}
