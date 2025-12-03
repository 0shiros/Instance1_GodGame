// StorageBuilding.cs
using UnityEngine;

public class StorageBuilding : MonoBehaviour
{
    [Header("Stock local (facultatif)")]
    public int storedWood = 0;
    public int storedStone = 0;
    public int storedFood = 0;

    // Méthodes simples pour dépôt / retrait
    public void Deposit(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Wood: storedWood += amount; break;
            case ResourceType.Stone: storedStone += amount; break;
            case ResourceType.Food: storedFood += amount; break;
        }
    }

    public int Withdraw(ResourceType type, int amount)
    {
        int withdrawn = 0;
        switch (type)
        {
            case ResourceType.Wood:
                withdrawn = Mathf.Min(amount, storedWood);
                storedWood -= withdrawn;
                break;
            case ResourceType.Stone:
                withdrawn = Mathf.Min(amount, storedStone);
                storedStone -= withdrawn;
                break;
            case ResourceType.Food:
                withdrawn = Mathf.Min(amount, storedFood);
                storedFood -= withdrawn;
                break;
        }
        return withdrawn;
    }
}
