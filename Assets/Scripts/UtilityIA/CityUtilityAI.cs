using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CityUtilityAI : MonoBehaviour
{
    [Header("Datas & Références")]
    public List<TaskData> TaskDataList = new List<TaskData>();
    public List<BuildingData> BuildingTypes = new List<BuildingData>();
    public E_Dogma CurrentDogma = E_Dogma.None;
    [SerializeField] private int agentsQuantityNeedToSetDogma;
    [SerializeField] private GameObject villager;
    public string cityName = "";

    [Header("Grid")]
    public GridManager2D GridManager;
    
    [Header("Villagers Statistics Limits")]
    public int HpMin = 85;
    public int HpMax = 100;
    public int SpeedMin = 5;
    public int SpeedMax = 20;
    public int StrengthMin = 5;
    public int StrengthMax = 20;

    [Header("Monde")]
    public Vector2Int GridSize = new Vector2Int(50, 50);
    public float TaskScanInterval = 0.2f;

    public AnimationCurve WorkerDistributionCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);
    public float MaxWorkerPercent = 0.5f;

    [Header("Placement bâtiments")]
    public float HouseSpawnDistance = 5f;
    public float BuildingMinDistance = 2f;

    [Header("Ressources globales")]
    public int TotalWood;
    public int TotalStone;
    public int TotalFood;
    public int TotalMetal;

    [Header("Stats bâtiments")]
    public int HousesBuilt = 0;


    [Header("Reproduction villageois")]
    public int MinFoodForReproduction = 10;
    public int MinHousesForReproduction = 2;
    public float ReproductionCooldown = 15f;
    public int MinBorn = 1;
    public int MaxBorn = 3;
    private float reproductionTimer = 0f;

   
    public List<VillagerUtilityAI> villagers = new List<VillagerUtilityAI>();
    private List<ResourceNode> resourceNodes = new List<ResourceNode>();
    private List<StorageBuilding> storages = new List<StorageBuilding>();
    public List<CityTask> ActiveTasks = new List<CityTask>();
    
    // Possible Names of nations/cities
    private readonly string[] nationNames = {
        
        "Avaloria", "Brumecity", "Celestia", "Draemor", "Eldoria", "Frosthaven", "Glimmerdale", "Harmonia", "Ironforge", "Jadewood",
        "Sylvandor", "Luneris", "Verdanelle", "Thalowyn", "Elarion", "Sylvaeris", "Ardanor", "Faelwyn", "Thornewood", "Mystralis",
        "Lysendell", "Evergrove", "Altheran", "Willowspire", "Amaranthil", "Frosthelm", "Nivorheim", "Skjoldvik", "Wintergate", "Icehaven", "Coldspire", "Hivernel", "Snowcrest", "Eldfrost", "Borealis Keep",
        "Whitehold", "Stonewatch", "Ironpeak", "Deepdelve", "Hammerhold", "Copperhall", "Graniteforge", "Darkstone", "Mithrildeep", "Emberhall",
        "Rocktide", "Solara", "Sandspire", "Mirazun", "Arkanesh", "Aridion", "Zahir’Kal", "Sunreach", "Dunespire", "Kalimora", "Orinar",
        "Seabreak", "Marivelle", "Driftport", "Tidescar", "Pelagia", "Stormshore", "Crestfall", "Oceanreach", "Saltwind", "Havenbay",
        "Kingsfall", "Highvalor", "Dawnmere", "Oakenguard", "Lioncrest", "Silverkeep", "Westford", "Varenholm", "Brightwall",
        "Greenreach", "Sunbrook", "Arcanis", "Mythrendale", "Etherwyn", "Astralis", "Nocturnia", "Luminor", "Tempys", "Shadovar", "Vesperia", "Enchantel",
        "Blackmoor", "Dreadfall", "Shadowfen", "Mor’Ghul", "Bloodfort", "Nightspire", "Ashencroft", "Thornkeep", "Voidreach",
        "Ravenhold", "Stormhold", "Moonspire", "Amberfall", "Dragonstead", "Goldshore", "Runebrook", "Falconcrest", "Oakshade", "Windrest",
        "Stonebrooke", "Riverhelm", "Wyrmwood", "Stormhollow", "Sunspire", "Ashenwald", "Clearhaven", "Faylen", "Northwyn",
        "Embertide", "Galehaven"
    };
    
    private float timer = 0f;
    private float debugTimer = 0f;

    [Header("Bâtiments de la ville")]
    public List<MonoBehaviour> CityBuildings = new List<MonoBehaviour>();


    public static Action<int> ActionBasic;
    public static Action<int> ActionDogma;

    [Header("Combat Decision")]
    public float AttackScanInterval = 10f;
    public float AttackRange = 50f;
    public float ResourceGreedFactor = 0.7f;
    public int MinVillagersToAttack = 4;

    private float attackScanTimer = 0f;




    #region Registration API (optimisation)


    public void RegisterVillager(VillagerUtilityAI v)
    {
        if (v == null) return;
        if (!villagers.Contains(v))
            villagers.Add(v);
        v.city = this;
    }


    public void UnregisterVillager(VillagerUtilityAI v)
    {
        if (v == null) return;
        if (villagers.Contains(v))
            villagers.Remove(v);
        if (v.city == this) v.city = null;
    }


    public void RegisterResourceNode(ResourceNode rn)
    {
        if (rn == null) return;
        if (!resourceNodes.Contains(rn))
            resourceNodes.Add(rn);
    }
    public void UnregisterCityBuilding(MonoBehaviour building)
    {
        if (building == null) return;
        CityBuildings.Remove(building);
    }
    bool ConsumeResourcesForBuilding(int wood, int stone, int food = 0)
    {
        if (TotalWood < wood || TotalStone < stone || TotalFood < food)
            return false;

        var validStorages = storages.FindAll(s =>
            s != null &&
            s.StoredWood >= wood &&
            s.StoredStone >= stone &&
            s.StoredFood >= food
        );

        if (validStorages.Count == 0)
            return false;

        var chosen = validStorages[UnityEngine.Random.Range(0, validStorages.Count)];

        chosen.Withdraw(ResourceType.Wood, wood);
        chosen.Withdraw(ResourceType.Stone, stone);
        if (food > 0)
            chosen.Withdraw(ResourceType.Food, food);

        TotalWood -= wood;
        TotalStone -= stone;
        TotalFood -= food;

        return true;
    }


    public void RegisterCityBuilding(MonoBehaviour building)
    {
        if (building == null) return;

        if (!CityBuildings.Contains(building))
        {
            CityBuildings.Add(building);
            if (CurrentDogma == E_Dogma.Craft)
            {
                AddDogmaSciencePoints(1);
            }
            AddSciencePoints(1);
        }
            
    }



    public void UnregisterResourceNode(ResourceNode rn)
    {
        if (rn == null) return;
        resourceNodes.Remove(rn);
    }


    public void RegisterStorage(StorageBuilding st)
    {
        if (st == null) return;
        if (!storages.Contains(st))
            storages.Add(st);
    }


    public void UnregisterStorage(StorageBuilding st)
    {
        if (st == null) return;
        storages.Remove(st);
    }

    #endregion



    private void Awake()
    {
        cityName = nationNames[UnityEngine.Random.Range(0, nationNames.Length)];
        gameObject.name = cityName;
        storages.Clear();
        storages.AddRange(GetComponentsInChildren<StorageBuilding>());
    }

    void Start()
    {
        if (GridManager == null)
        {
            GridManager = FindObjectOfType<GridManager2D>();
            if (GridManager == null) ;
           
        }

        RefreshSceneListsForce();
        AggregateStorage();

        AddVillagers(6);
    }

    private void AddVillagers(int pQuantity)
    {
        for (int i = 0; i < pQuantity; i++)
        {
            var go = Instantiate(villager, transform);
            var human= go.GetComponent<VillagerUtilityAI>();
            if (human != null) RegisterVillager(human);
            human.Hp = Random.Range(HpMin, HpMax);
            human.agent.speed = Random.Range(SpeedMin, SpeedMax);
            human.Strength = Random.Range(StrengthMin, StrengthMax);
        }
        SetDogma();
    }
    
    public void AddStrengthToAllVillagers(int pStrenghtBonus)
    {
        foreach (var villager in villagers)
        {
            villager.Strength += pStrenghtBonus;
        }
        
        StrengthMin += pStrenghtBonus;
        StrengthMax += pStrenghtBonus;
    }
    
    public void AddHealthToAllVillagers(int pHpBonus)
    {
        foreach (var villager in villagers)
        {
            villager.Hp += pHpBonus;
        }
        
        HpMin += pHpBonus;
        HpMax += pHpBonus;
    }
    
    public void AddSpeedToAllVillagers(int pSpeedBonus)
    {
        foreach (var villager in villagers)
        {
            villager.agent.speed += pSpeedBonus;
        }
        
        SpeedMin += pSpeedBonus;
        SpeedMax += pSpeedBonus;
    }


    void Update()
    {
        timer += Time.deltaTime;
        debugTimer += Time.deltaTime;

        
        reproductionTimer += Time.deltaTime;
        if (reproductionTimer >= ReproductionCooldown)
        {
            reproductionTimer = 0f;
            TryReproduce();
        }

        TryRecruitNearbyVillagers();

        if (timer >= TaskScanInterval)
        {
            timer = 0f;
            CleanupFinishedTasks();
            RefreshSceneListsIfNeeded();
            HandleResourceTasks();
            HandleBuildTasks();
            AssignTasksToVillagers();
            AggregateStorage();
        }

        if (debugTimer >= 1f)
        {
            debugTimer = 0f;
            
        }
        attackScanTimer += Time.deltaTime;
        if (attackScanTimer >= AttackScanInterval)
        {
            attackScanTimer = 0f;
            EvaluateAttackOpportunity();
        }
    }
    float GetTotalResources()
    {
        return TotalWood + TotalStone + TotalFood + TotalMetal;
    }

    void EvaluateAttackOpportunity()
    {
        if (villagers.Count < MinVillagersToAttack)
            return;

        CityUtilityAI bestTarget = null;
        float bestScore = float.NegativeInfinity;

        var allCities = FindObjectsByType<CityUtilityAI>(FindObjectsSortMode.None);

        foreach (var other in allCities)
        {
            if (other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist > AttackRange) continue;

            float myPower = villagers.Count;
            float enemyPower = other.villagers.Count;

            if (enemyPower <= 0) continue;

            float resourceScore =
                other.GetTotalResources()
                - (GetTotalResources() * ResourceGreedFactor);

            float powerScore = (myPower - enemyPower) * 10f;

            float finalScore = resourceScore + powerScore;

            if (finalScore > bestScore)
            {
                bestScore = finalScore;
                bestTarget = other;
            }
        }

        if (bestTarget != null && bestScore > 0f)
        {
            OrderAttack(bestTarget);
        }
    }


    private void TryReproduce()
    {
        if (TotalFood < MinFoodForReproduction) return;
        if (HousesBuilt < MinHousesForReproduction) return;

        int born = UnityEngine.Random.Range(MinBorn, MaxBorn + 1);

        for (int i = 0; i < born; i++)
        {
            var go = Instantiate(villager, transform);
            var v = go.GetComponent<VillagerUtilityAI>();
            if (v != null) RegisterVillager(v);
        }

       
        TotalFood -= MinFoodForReproduction;

        
    }





    void CleanupFinishedTasks()
    {
        ActiveTasks.RemoveAll(t => t == null || t.IsCompleted);
    }


   



#region Tâches / Création



    void HandleResourceTasks()
    {
        foreach (var node in resourceNodes)
        {
            if (node == null || node.Amount <= 0) continue;

            bool exists = ActiveTasks.Exists(t =>
                t.Data != null &&
                t.Data.Type == TaskType.Collect &&
                t.ResourceTarget == node);

            if (exists) continue;

            var collectData = TaskDataList.Find(td =>
                td.Type == TaskType.Collect &&
                td.TargetResource == node.ResourceType);

            if (collectData == null) continue;

            ActiveTasks.Add(new CityTask
            {
                Data = collectData,
                ResourceTarget = node
            });
        }
    }

    void HandleBuildTasks()
    {
        if (GridManager == null) return;

        foreach (var building in BuildingTypes)
        {
            if (building == null || building.Prefab == null) continue;

            bool alreadyPlanned = ActiveTasks.Exists(t =>
                t.Data != null &&
                t.Data.Type == TaskType.Build &&
                t.BuildingData == building);

            if (alreadyPlanned) continue;

            if (TotalWood >= building.WoodCost && TotalStone >= building.StoneCost)
            {
                Vector2Int targetCell;
                bool foundCell = false;

               
                if (GridManager.GetCellsOwnedByCity(this).Count == 0)
                {
                    foundCell = GridManager.TryFindNearestFreeCell(transform.position, building.Size, out targetCell);
                }
                else
                {
                    foundCell = GridManager.TryFindCellAdjacentToCity(this, building, out targetCell)
                                || GridManager.TryFindRandomCellAroundHouse(building.Size, out targetCell);
                }

                if (!foundCell) continue;

               
                Vector3 worldPos = GridManager.CellToWorld(targetCell.x, targetCell.y);
                if (!UnityEngine.AI.NavMesh.SamplePosition(worldPos, out UnityEngine.AI.NavMeshHit hit, 0.1f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    continue; 
                }
                worldPos = hit.position;

               
                if (GridManager.TryReserveCell(this, targetCell, building, out Vector3 reservedWorldPos))
                {
                    reservedWorldPos += new Vector3(
                        UnityEngine.Random.Range(-0.4f, 0.4f) * GridManager.CellSize,
                        UnityEngine.Random.Range(-0.4f, 0.4f) * GridManager.CellSize,
                        0f
                    );
                    if (!ConsumeResourcesForBuilding(building.WoodCost, building.StoneCost))
                    {
                        GridManager.ReleaseReservation(targetCell);
                        continue;
                    }




                    var buildTaskData = TaskDataList.Find(td =>
                        td.Type == TaskType.Build &&
                        td.TargetBuildingType == building.BuildingType);

                    if (buildTaskData == null)
                    {
                        GridManager.ReleaseReservation(targetCell);
                        continue;
                    }

                    CityTask t = new CityTask
                    {
                        Data = buildTaskData,
                        BuildingData = building,
                        BuildPosition = reservedWorldPos
                    };

                    ActiveTasks.Add(t);

                    if (building.BuildingType == BuildingType.House)
                    {
                        HousesBuilt++;
                       
                    }
                }
            }
        }
    }


    #endregion

    #region Assignation

    void AssignTasksToVillagers()
    {
        foreach (var villager in villagers)
        {
            if (villager == null) continue;
            if (!villager.IsIdle()) continue;
            AssignTaskToVillager(villager);
        }
    }

    public void AssignTaskToVillager(VillagerUtilityAI pVillager)
    {
        AssignTaskToVillager(pVillager, null);
    }

    public void AssignTaskToVillager(VillagerUtilityAI pVillager, CityTask pForced)
    {
        if (pVillager == null || !pVillager.IsIdle()) return;

        CityTask best = null;
        float bestScore = float.NegativeInfinity;

        foreach (var task in ActiveTasks)
        {
            if (task == null || task.IsCompleted) continue;

            int recommended = ComputeRecommendedWorkers(task);
            if (task.AassignedVillagers.Count >= recommended) continue;

            float score = ScoreTaskForVillager(task, pVillager);
            if (score > bestScore)
            {
                bestScore = score;
                best = task;
            }
        }

        if (pForced != null && !pForced.IsCompleted)
            best = pForced;

        if (best != null)
        {
            if (!best.AassignedVillagers.Contains(pVillager))
                best.AassignedVillagers.Add(pVillager);

            pVillager.AssignTask(best);
           
        }
    }

    float ScoreTaskForVillager(CityTask pTask, VillagerUtilityAI pVillager)
    {
        float score = (pTask.Data != null) ? pTask.Data.BasePriority : 0f;
        if (pTask.Data == null) return score;

        if (pTask.Data.Type == TaskType.Collect && pTask.ResourceTarget != null)
        {
            if (pTask.ResourceTarget.ResourceType == ResourceType.Wood && TotalWood < 5) score += 10f;
            if (pTask.ResourceTarget.ResourceType == ResourceType.Stone && TotalStone < 3) score += 8f;

            float dist = Vector3.Distance(pVillager.transform.position, pTask.ResourceTarget.transform.position);
            score -= dist * 0.1f;
        }
        else if (pTask.Data.Type == TaskType.Build && pTask.BuildingData != null)
        {
            float dist = Vector3.Distance(pVillager.transform.position, pTask.BuildPosition);
            score -= dist * 0.05f;
        }

        if (pVillager.role == VillagerRole.Gatherer && pTask.Data.Type == TaskType.Collect) score += 5f;
        if (pVillager.role == VillagerRole.Builder && pTask.Data.Type == TaskType.Build) score += 5f;

        score += UnityEngine.Random.Range(-0.5f, 0.5f);
        return score;
    }

    #endregion

    #region Helpers

    void TryRecruitNearbyVillagers()
    {
       
        Vector3 cityCenter = transform.position;
        float recruitRadius = 15f;

       
       Collider[] cols = Physics.OverlapSphere(cityCenter, recruitRadius);
        foreach (var col in cols)
        {
            if (col == null) continue;
            var v = col.GetComponent<VillagerUtilityAI>();
            if (v == null) continue;

           
            if (villagers.Contains(v) || v.city != null) continue;

            
            RegisterVillager(v);
           
        }

       
        villagers.RemoveAll(x => x == null);
    }



    void AggregateStorage()
    {

        int w = 0, s = 0, f = 0;
        foreach (var st in storages)
        {
            if (st == null) continue;
            w += st.StoredWood;
            s += st.StoredStone;
            f += st.StoredFood;
            if(CurrentDogma== E_Dogma.Development)
            {
                AddDogmaSciencePoints(1);
            }
            AddSciencePoints(1);
        }
        TotalWood = w;
        TotalStone = s;
        TotalFood = f;
    }


    public void NotifyResourceCollected(ResourceType pType, int pAmount)
    {
        if (pAmount <= 0) return;

        switch (pType)
        {
            case ResourceType.Wood: TotalWood += pAmount; break;
            case ResourceType.Stone: TotalStone += pAmount; break;
            case ResourceType.Food: TotalFood += pAmount; break;
        }
    }

    public StorageBuilding FindNearestStorage(Vector3 pFrom)
    {
        StorageBuilding best = null;
        float bestDist = float.PositiveInfinity;

        foreach (var s in storages)
        {
            if (s == null) continue;

            float d = Vector3.Distance(pFrom, s.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = s;
            }
        }

        return best;
    }

    public void debugActiveTasks()
    {
        if (ActiveTasks == null || ActiveTasks.Count == 0)
        {
           
            return;
        }

        string log = "[City] Tâches actives: ";

        foreach (var t in ActiveTasks)
        {
            if (t == null || t.Data == null) continue;

            string target = t.ResourceTarget != null ? t.ResourceTarget.name :
                            t.BuildingData != null ? t.BuildingData.BuildingName :
                            "N/A";

            log += $"[{t.Data.TaskName}:{t.Data.Type}->{target} assigned:{t.AassignedVillagers.Count}] ";
        }

        Debug.Log(log);
    }

    private float slowRefreshTimer = 0f;
    private const float slowRefreshInterval = 30f; // toutes les 30 secondes

    void RefreshSceneListsIfNeeded()
    {
        
        resourceNodes.RemoveAll(x => x == null);
        villagers.RemoveAll(x => x == null);


   
    }


    void RefreshSceneListsForce()
    {
        resourceNodes = FindObjectsOfType<ResourceNode>().ToList();


    }



    #endregion

    #region Worker calculation (AnimationCurve)

    public int ComputeRecommendedWorkers(CityTask pTask)
    {
        if (pTask == null || pTask.Data == null) return 1;

        int population = Mathf.Max(1, villagers.Count);

        float normalizedPriority = Mathf.InverseLerp(0f, 10f, pTask.Data.BasePriority);

        float distanceFactor = 0f;
        if (pTask.ResourceTarget != null)
        {
            float avgDist = villagers
                .Where(v => v != null)
                .Average(v => Vector3.Distance(v.transform.position, pTask.ResourceTarget.transform.position));
            distanceFactor = Mathf.Clamp01(avgDist / 30f);
        }

        float utilityScore = Mathf.Clamp01(normalizedPriority * 0.7f + distanceFactor * 0.3f);
        float percent = WorkerDistributionCurve.Evaluate(utilityScore);
        percent = Mathf.Clamp(percent, 0f, MaxWorkerPercent);

        int recommended = Mathf.Max(1, Mathf.RoundToInt(population * percent));
        return recommended;
    }

    #endregion

    #region API publique

    public void RemoveTask(CityTask pTask)
    {
        if (pTask == null) return;
        if (ActiveTasks.Contains(pTask))
            ActiveTasks.Remove(pTask);
    }

    #endregion

    #region CalculateAverages / SetDogma / AddSciencePoints

    private float[] CalculateAverages()
    {
        float totalHpNormalized = 0;
        float totalSpeedNormalized = 0;
        float totalStrengthNormalized = 0;

        foreach (var agent in villagers)
        {
            float hpNormalized = ((agent.Hp - HpMin));
            float speedNormalized = ((agent.agent.speed - SpeedMin));
            float strengthNormalized = ((agent.Strength - StrengthMin));

            totalHpNormalized += hpNormalized;
            totalSpeedNormalized += speedNormalized;
            totalStrengthNormalized += strengthNormalized;
        }

        float hpPercent = totalHpNormalized / ((HpMax - HpMin) * villagers.Count);
        float speedPercent = totalSpeedNormalized / ((SpeedMax - SpeedMin) * villagers.Count);
        float strengthPercent = totalStrengthNormalized / ((StrengthMax - StrengthMin) * villagers.Count);

       

        return new float[] { hpPercent, speedPercent, strengthPercent };
    }

    private void SetDogma()
    {
        if (villagers.Count < agentsQuantityNeedToSetDogma)
            return;

        float[] AveragePercents = CalculateAverages();

        int maxIndex = 0;
        for (int i = 1; i < AveragePercents.Length; i++)
        {
            if (AveragePercents[i] > AveragePercents[maxIndex])
                maxIndex = i;
        }

        switch (maxIndex)
        {
            case 0: CurrentDogma = E_Dogma.Craft; break;
            case 1: CurrentDogma = E_Dogma.Development; break;
            case 2: CurrentDogma = E_Dogma.Military; break;
        }
    }

    public void AddSciencePoints(int pExperienceReward)
    {
        ActionBasic?.Invoke(pExperienceReward);
    }

    public void AddDogmaSciencePoints(int pExperienceReward)
    {
        ActionDogma?.Invoke(pExperienceReward);
    }

    #endregion
    public void OrderAttack(CityUtilityAI targetCity)
    {
        if (targetCity == null) return;

        CityCombatController myCombat = GetComponent<CityCombatController>();
        CityCombatController enemyCombat = targetCity.GetComponent<CityCombatController>();

        if (myCombat == null || enemyCombat == null)
        {
            
            return;
        }

       
    }

}
