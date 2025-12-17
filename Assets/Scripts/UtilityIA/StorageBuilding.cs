
using UnityEngine;

public class StorageBuilding : MonoBehaviour
{
    [Header("Stock local (facultatif)")]
    public int StoredWood = 0;
    public int StoredStone = 0;
    public int StoredFood = 0;
    public int StoredMetal = 0;
    
    public CityUtilityAI city;
    
    public int Withdraw(ResourceType pType, int pAmount)
    {
        int withdrawn = 0;
        switch (pType)
        {
            case ResourceType.Wood:
                withdrawn = Mathf.Min(pAmount, StoredWood);
                city.TotalWood -= withdrawn;
                break;
            case ResourceType.Stone:
                withdrawn = Mathf.Min(pAmount, StoredStone);
                city.TotalStone -= withdrawn;
                break;
            case ResourceType.Food:
                withdrawn = Mathf.Min(pAmount, StoredFood);
                city.TotalFood -= withdrawn;
                break;
            case ResourceType.Metal:
                withdrawn = Mathf.Min(pAmount, StoredMetal);
                city.TotalMetal -= withdrawn;
                break;
        }
        return withdrawn;
    }
}
