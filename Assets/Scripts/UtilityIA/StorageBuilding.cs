
using UnityEngine;

public class StorageBuilding : MonoBehaviour
{
    [Header("Stock local (facultatif)")]
    public int StoredWood = 0;
    public int StoredStone = 0;
    public int StoredFood = 0;

    public void Deposit(ResourceType pType, int pAmount)
    {
        switch (pType)
        {
            case ResourceType.Wood: StoredWood += pAmount; break;
            case ResourceType.Stone: StoredStone += pAmount; break;
            case ResourceType.Food: StoredFood += pAmount; break;
        }
    }

    public int Withdraw(ResourceType pType, int pAmount)
    {
        int withdrawn = 0;
        switch (pType)
        {
            case ResourceType.Wood:
                withdrawn = Mathf.Min(pAmount, StoredWood);
                StoredWood -= withdrawn;
                break;
            case ResourceType.Stone:
                withdrawn = Mathf.Min(pAmount, StoredStone);
                StoredStone -= withdrawn;
                break;
            case ResourceType.Food:
                withdrawn = Mathf.Min(pAmount, StoredFood);
                StoredFood -= withdrawn;
                break;
        }
        return withdrawn;
    }
}
