using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityCombatController : MonoBehaviour
{
    [Header("Owner")]
    public CityUtilityAI OwnerCity { get; private set; }

    public CityUtilityAI enemyCity;

    public List<VillagerUtilityAI> attackers = new();
    public List<VillagerUtilityAI> defenders = new();

    private bool combatRunning = false;

    [Header("Combat")]
    [SerializeField] private Transform combatCenter;
    private List<MonoBehaviour> enemyBuildings = new();

    [Header("Combat Settings")]
    public float moveSpeed = 3f;
    public float attackDelay = 0.5f;
    public float betweenTurnsDelay = 0.3f;
    public float combatOffset = 1.5f;

    private void Awake()
    {
        OwnerCity = GetComponent<CityUtilityAI>();
        if (OwnerCity == null)
            Debug.LogError("[Combat] CityUtilityAI manquant sur le GameObject");

        if (combatCenter == null)
            combatCenter = transform;
    }

    public void StartCombat(CityUtilityAI targetCity)
    {
        if (combatRunning || targetCity == null) return;

        enemyBuildings = targetCity.CityBuildings.Where(b => b != null).ToList();
        enemyCity = targetCity;

        attackers = OwnerCity.villagers.FindAll(v => v != null);
        defenders = targetCity.villagers.FindAll(v => v != null);

        foreach (var v in attackers)
            v.EnterCombat(enemyCity.transform.position, true);

        combatRunning = true;

        Debug.Log($"⚔️ {OwnerCity.cityName} attaque {targetCity.cityName}");

        StartCoroutine(CombatRoutine());
    }

    public void StartDefense(CityUtilityAI attackerCity)
    {
        if (combatRunning || attackerCity == null) return;

        enemyCity = attackerCity;

        attackers = attackerCity.villagers.FindAll(v => v != null);
        defenders = OwnerCity.villagers.FindAll(v => v != null);

        foreach (var v in attackers)
            v.EnterCombat(OwnerCity.transform.position, true);

        foreach (var v in defenders)
            v.EnterCombat(OwnerCity.transform.position, false);

        combatRunning = true;

        Debug.Log($"🛡️ {OwnerCity.cityName} se défend contre {attackerCity.cityName}");

        StartCoroutine(CombatRoutine());
    }

    private IEnumerator CombatRoutine()
    {
       
        for (int i = 0; i < attackers.Count; i++)
            attackers[i].transform.position = combatCenter.position + new Vector3(-combatOffset, i * 1f, 0);

        for (int i = 0; i < defenders.Count; i++)
            defenders[i].transform.position = combatCenter.position + new Vector3(combatOffset, i * 1f, 0);

        while (attackers.Any(a => a != null && a.Hp > 0) && defenders.Any(d => d != null && d.Hp > 0))
        {
           
           attackers = attackers.Where(a => a != null && a.Hp > 0).ToList();
            defenders = defenders.Where(d => d != null && d.Hp > 0).ToList();

            int maxCount = Mathf.Max(attackers.Count, defenders.Count);

            for (int i = 0; i < maxCount; i++)
            {
                if (i < attackers.Count && defenders.Count > 0)
                {
                    var attacker = attackers[i];
                    var defender = defenders[Random.Range(0, defenders.Count)];
                    yield return StartCoroutine(PerformAttack(attacker, defender));
                }

                if (i < defenders.Count && attackers.Count > 0)
                {
                    var defender = defenders[i];
                    var attacker = attackers[Random.Range(0, attackers.Count)];
                    yield return StartCoroutine(PerformAttack(defender, attacker));
                }
            }
        }


        EndCombat();
    }


    private IEnumerator PerformAttack(VillagerUtilityAI attacker, VillagerUtilityAI defender)
    {
        if (attacker == null || defender == null) yield break;

        Vector3 originalPos = attacker.transform.position;
        Vector3 attackPos = defender.transform.position + Vector3.left * 0.5f * Mathf.Sign(attacker.transform.position.x - defender.transform.position.x);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            if(attacker!=null)
            {
                attacker.transform.position = Vector3.Lerp(originalPos, attackPos, t);
            }
        
            yield return null;
        }

        yield return new WaitForSeconds(attackDelay);

        if (defender != null && attacker != null) {
            defender.TakeDamage(attacker.Strength);
            Debug.Log($"{attacker.name} inflige {attacker.Strength} à {defender.name}");
        }
        
        

       
        if (defender.Hp <= 0)
        {
            defender.Die();
        }

       
       t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            if(attacker!=null)
            {
                attacker.transform.position = Vector3.Lerp(attackPos, originalPos, t);
            }
           
            yield return null;
        }

        yield return new WaitForSeconds(betweenTurnsDelay);
    }


    private void EndCombat()
    {
        combatRunning = false;

        
        attackers = attackers.Where(a => a != null && a.Hp > 0).ToList();
        defenders = defenders.Where(d => d != null && d.Hp > 0).ToList();

       
        foreach (var v in attackers)
            v.ExitCombat();
        foreach (var v in defenders)
            v.ExitCombat();

        if (enemyCity != null && OwnerCity.villagers.Count > 0)
        {
            LootEnemyCity(enemyCity);
           
            
        }

        attackers.Clear();
        defenders.Clear();
        enemyBuildings.Clear();

        enemyCity = null;
    }


    private void LootEnemyCity(CityUtilityAI defeatedCity)
    {
        OwnerCity.TotalWood += defeatedCity.TotalWood;
        OwnerCity.TotalStone += defeatedCity.TotalStone;
        OwnerCity.TotalFood += defeatedCity.TotalFood;
        OwnerCity.TotalMetal += defeatedCity.TotalMetal;

        defeatedCity.TotalWood = 0;
        defeatedCity.TotalStone = 0;
        defeatedCity.TotalFood = 0;
        defeatedCity.TotalMetal = 0;
        if (OwnerCity.CurrentDogma == E_Dogma.Military)
        {
            OwnerCity.AddDogmaSciencePoints(1);
        }
        OwnerCity.AddDogmaSciencePoints(1);
        Destroy(enemyCity.gameObject);
        Debug.Log($"💰 {OwnerCity.cityName} pille toutes les ressources de {defeatedCity.cityName}");
    }
}
