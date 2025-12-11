// ResourceNode.cs
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [Header("Ressource")]
    public ResourceType ResourceType = ResourceType.Wood;
    public int Amount = 10;              
    public int HarvestPerAction = 1;     

   
}
