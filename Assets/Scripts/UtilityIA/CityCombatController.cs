using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CityCombatController : MonoBehaviour
{
    [Header("Owner")]
    public CityUtilityAI OwnerCity { get; private set; }

    public CityUtilityAI enemyCity;

    public List<villagersUtilityAI> attackers = new();
    public List<villagersUtilityAI> defenders = new();

    private bool combatRunning = false;

    [Header("Combat")]
    [SerializeField] private Transform combatCenter;

    private void Awake()
    {
        OwnerCity = GetComponent<CityUtilityAI>();
        if (OwnerCity == null)
            Debug.LogError("[Combat] CityUtilityAI manquant sur le GameObject");

        if (combatCenter == null)
            combatCenter = transform;
    }

    /// <summary>
    /// Vérifie si un chemin valide existe sur le NavMesh entre les deux villes
    /// </summary>
    private bool CanReachCity(Vector3 from, Vector3 to)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }

    public void StartCombat(CityUtilityAI targetCity)
    {
        if (combatRunning || targetCity == null) return;

        // Vérification du NavMesh
        if (!CanReachCity(transform.position, targetCity.transform.position))
        {
            Debug.LogWarning($"❌ {OwnerCity.cityName} ne peut pas attaquer {targetCity.cityName} : pas de chemin sur le NavMesh !");
            return;
        }

        enemyCity = targetCity;

        attackers = OwnerCity.villagers.Where(v => v != null).ToList();
        defenders = targetCity.villagers.Where(v => v != null).ToList();

        foreach (var v in attackers)
            v?.EnterCombat(targetCity.transform.position, true);

        foreach (var v in defenders)
            v?.EnterCombat(OwnerCity.transform.position, false);

        combatRunning = true;
        Debug.Log($"⚔️ {OwnerCity.cityName} attaque {targetCity.cityName}");

        StartCoroutine(CombatRoutine());
    }

    public void StartDefense(CityUtilityAI attackerCity)
    {
        if (combatRunning || attackerCity == null) return;

        // Vérification du NavMesh
        if (!CanReachCity(transform.position, attackerCity.transform.position))
        {
            Debug.LogWarning($"❌ {OwnerCity.cityName} ne peut pas se défendre contre {attackerCity.cityName} : pas de chemin sur le NavMesh !");
            return;
        }

        enemyCity = attackerCity;

        attackers = attackerCity.villagers.Where(v => v != null).ToList();
        defenders = OwnerCity.villagers.Where(v => v != null).ToList();

        foreach (var v in attackers)
            v?.EnterCombat(OwnerCity.transform.position, true);

        foreach (var v in defenders)
            v?.EnterCombat(OwnerCity.transform.position, false);

        combatRunning = true;
        Debug.Log($"🛡️ {OwnerCity.cityName} se défend contre {attackerCity.cityName}");

        StartCoroutine(CombatRoutine());
    }

    private IEnumerator CombatRoutine()
    {
        while (attackers.Any(a => a != null && a.Hp > 0) && defenders.Any(d => d != null && d.Hp > 0))
        {
            attackers = attackers.Where(a => a != null && a.Hp > 0).ToList();
            defenders = defenders.Where(d => d != null && d.Hp > 0).ToList();

            yield return null;
        }

        EndCombat();
    }

    private void EndCombat()
    {
        combatRunning = false;

        attackers = attackers.Where(a => a != null && a.Hp > 0).ToList();
        defenders = defenders.Where(d => d != null && d.Hp > 0).ToList();

        foreach (var v in attackers)
            v?.ExitCombat();
        foreach (var v in defenders)
            v?.ExitCombat();

        if (enemyCity != null && OwnerCity.villagers.Count > 0)
            LootEnemyCity(enemyCity);

        attackers.Clear();
        defenders.Clear();
        enemyCity = null;
    }

    private void LootEnemyCity(CityUtilityAI defeatedCity)
    {
        if (defeatedCity == null) return;

        float lootWood = defeatedCity.TotalWood;
        float lootStone = defeatedCity.TotalStone;
        float lootFood = defeatedCity.TotalFood;
        float lootMetal = defeatedCity.TotalMetal;

        OwnerCity.TotalWood += lootWood;
        OwnerCity.TotalStone += lootStone;
        OwnerCity.TotalFood += lootFood;
        OwnerCity.TotalMetal += lootMetal;

        defeatedCity.TotalWood = 0;
        defeatedCity.TotalStone = 0;
        defeatedCity.TotalFood = 0;
        defeatedCity.TotalMetal = 0;

        if (OwnerCity.CurrentDogma == E_Dogma.Military)
            OwnerCity.AddDogmaSciencePoints(1);

        OwnerCity.AddDogmaSciencePoints(1);

        ParticleManager.Instance?.StartParticle(0);

        if (defeatedCity.gameObject != null)
            Destroy(defeatedCity.gameObject);

        Debug.Log($"💰 {OwnerCity.cityName} pille toutes les ressources de {defeatedCity.cityName}");
    }
}
