using UnityEngine;

public class BuildingCombat : MonoBehaviour
{
    public int Hp = 80;

    public void TakeDamage(int amount)
    {
        Hp -= amount;
        Hp = Mathf.Max(Hp, 0);

        if (Hp <= 0)
            DestroyBuilding();
    }

    private void DestroyBuilding()
    {
        // Effet ou animation possible ici
        Destroy(gameObject);
    }
}
