// ResourceNode.cs
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [Header("Ressource")]
    public ResourceType resourceType = ResourceType.Wood;
    public int amount = 10;              // quantité restante
    public int harvestPerAction = 1;     // ce que prend un villageois par collecte

    // Optionnel : id / qualité / respawn => peut être ajouté plus tard
}
