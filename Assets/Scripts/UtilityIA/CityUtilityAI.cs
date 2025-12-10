using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

/// <summary>
/// CityUtilityAI
/// - Scanne la scène (villageois, nodes, storages)
/// - Génère des CityTask (collect, build)
/// - Assigne selon un score Utility
/// - Agrège les ressources depuis les StorageBuildings
/// - Calcule automatiquement le nombre recommandé de travailleurs via une AnimationCurve
/// </summary>
public class CityUtilityAI : MonoBehaviour
{
    [Header("Datas & Références")]
    public List<TaskData> taskDataList = new List<TaskData>();
    public List<BuildingData> buildingTypes = new List<BuildingData>();
    public E_Dogma CurrentDogma = E_Dogma.None;
    public int AgentsQuantity;
    [SerializeField] private int agentsQuantityNeedToSetDogma;
    [SerializeField] private GameObject villager;
    public string cityName = "";

    [Header("Monde")]
    public Vector2Int gridSize = new Vector2Int(50, 50);
    public float taskScanInterval = 0.2f; // secondes

    [Header("Placement bâtiments")]
    public float houseSpawnDistance = 5f;                 // distance autour d’une maison existante
    public float buildingMinDistance = 2f;                // empêche les collisions de bâtiments

    [Header("Ressources globales (agrégées des dépôts)")]
    public int TotalWood;
    public int TotalStone;
    public int TotalFood;
    public int TotalMetal;

    [Header("Réglages distribution travailleurs")]
    [Tooltip("Courbe (0..1) -> pourcentage de population assignable (0..1).")]
    public AnimationCurve workerDistributionCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);
    [Tooltip("Pourcentage max (0..1) de population pouvant être assigné à une tâche.")]
    [Range(0f, 1f)]
    public float maxWorkerPercent = 0.5f;

    // internes
    public List<VillagerUtilityAI> villagers = new List<VillagerUtilityAI>();
    private List<ResourceNode> resourceNodes = new List<ResourceNode>();
    private List<StorageBuilding> storages = new List<StorageBuilding>();
    public List<CityTask> activeTasks = new List<CityTask>();

    private float timer = 0f;
    private float debugTimer = 0f;

    public static Action<int> actionBasic;
    public static Action<int> actionDogma;

    private void Awake()
    {
        cityName = gameObject.name;
    }

    void Start()
    {
        AddVillagers(6);
        RefreshSceneListsForce();
        AggregateStorage();
        SetDogma();
    }

    private void AddVillagers(int pQuantity)
    {
        for(int i = 0; i < pQuantity; i++)
        {
            Instantiate(villager, transform);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        debugTimer += Time.deltaTime;

        // TryRecruitNewVillagers
        TryRecruitNearbyVillagers();

        if (timer >= taskScanInterval)
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
            DebugActiveTasks();
        }
    }

    #region Tâches / Création

    void CleanupFinishedTasks()
    {
        activeTasks.RemoveAll(t => t == null || t.isCompleted);
    }

    void HandleResourceTasks()
    {
        foreach (var node in resourceNodes)
        {
            if (node == null || node.amount <= 0) continue;

            bool exists = activeTasks.Exists(t => t.data != null && t.data.type == TaskType.Collect && t.resourceTarget == node);
            if (exists) continue;

            var collectData = taskDataList.Find(td => td.type == TaskType.Collect && td.targetResource == node.resourceType);
            if (collectData == null) continue;

            var newT = new CityTask
            {
                data = collectData,
                resourceTarget = node
            };
            activeTasks.Add(newT);
        }
    }

    void HandleBuildTasks()
    {
        foreach (var building in buildingTypes)
        {
            if (building == null || building.prefab == null) continue;

            bool alreadyPlanned = activeTasks.Exists(t => t.data != null && t.data.type == TaskType.Build && t.buildingData == building);
            if (alreadyPlanned) continue;

            if (TotalWood >= building.woodCost && TotalStone >= building.stoneCost)
            {
                if (FindFreeBuildPositionRandomAroundHouse(building.size, out Vector3 pos)) // ★ AJOUT
                {
                    var buildTaskData = taskDataList.Find(td => td.type == TaskType.Build && td.targetBuildingType == building.buildingType);
                    if (buildTaskData == null) continue;

                    CityTask t = new CityTask
                    {
                        data = buildTaskData,
                        buildingData = building,
                        buildPosition = pos
                    };
                    activeTasks.Add(t);

                    TotalWood -= building.woodCost;
                    TotalStone -= building.stoneCost;
                }
            }
        }
    }

    void HandleDepositTasks() { }

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

    public void AssignTaskToVillager(VillagerUtilityAI villager)
    {
        AssignTaskToVillager(villager, null);
    }

    public void AssignTaskToVillager(VillagerUtilityAI villager, CityTask forced)
    {
        if (villager == null || !villager.IsIdle()) return;

        CityTask best = null;
        float bestScore = float.NegativeInfinity;

        foreach (var task in activeTasks)
        {
            if (task == null || task.isCompleted) continue;

            int recommended = ComputeRecommendedWorkers(task);
            if (task.assignedVillagers.Count >= recommended) continue;

            float score = ScoreTaskForVillager(task, villager);
            if (score > bestScore)
            {
                bestScore = score;
                best = task;
            }
        }

        if (forced != null && !forced.isCompleted)
            best = forced;

        if (best != null)
        {
            if (!best.assignedVillagers.Contains(villager))
                best.assignedVillagers.Add(villager);

            villager.AssignTask(best);
            Debug.Log($"[City] AssignTask: {villager.name} -> {best.data.taskName} ({best.data.type})");
        }
    }

    float ScoreTaskForVillager(CityTask task, VillagerUtilityAI villager)
    {
        float score = (task.data != null) ? task.data.basePriority : 0f;
        if (task.data == null) return score;

        if (task.data.type == TaskType.Collect && task.resourceTarget != null)
        {
            if (task.resourceTarget.resourceType == ResourceType.Wood && TotalWood < 5) score += 10f;
            if (task.resourceTarget.resourceType == ResourceType.Stone && TotalStone < 3) score += 8f;

            float dist = Vector3.Distance(villager.transform.position, task.resourceTarget.transform.position);
            score -= dist * 0.1f;
        }
        else if (task.data.type == TaskType.Build && task.buildingData != null)
        {
            float dist = Vector3.Distance(villager.transform.position, task.buildPosition);
            score -= dist * 0.05f;
        }

        if (villager.role == VillagerRole.Gatherer && task.data.type == TaskType.Collect) score += 5f;
        if (villager.role == VillagerRole.Builder && task.data.type == TaskType.Build) score += 5f;

        score += UnityEngine.Random.Range(-0.5f, 0.5f);
        return score;
    }

    #endregion

    #region Helpers

    // recrute un villageois si proche du centre du village
    void TryRecruitNearbyVillagers()
    {
        Vector3 cityCenter = transform.position;

        foreach (var v in FindObjectsOfType<VillagerUtilityAI>())
        {
            if (villagers.Contains(v)) continue;

            float d = Vector3.Distance(v.transform.position, cityCenter);
            if (d < 15f) // distance de recrutement
            {
                villagers.Add(v);
                Debug.Log($"[City] Nouveau villageois rejoint le village : {v.name}");
            }
        }
    }

    // nouvelle version du placement random autour d'une maison
    bool FindFreeBuildPositionRandomAroundHouse(Vector2Int size, out Vector3 pos)
    {
        pos = Vector3.zero;

        GameObject[] existingHouses = GameObject.FindGameObjectsWithTag("Building");
        if (existingHouses.Length == 0)
            return FindFreeBuildPosition(size, out pos);

        GameObject baseHouse = existingHouses[UnityEngine.Random.Range(0, existingHouses.Length)];

        for (int i = 0; i < 50; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * houseSpawnDistance;
            Vector3 candidate = baseHouse.transform.position + new Vector3(offset.x, offset.y, 0f);

            if (!Physics2D.OverlapCircle(candidate, buildingMinDistance))
            {
                pos = candidate;
                return true;
            }
        }

        return FindFreeBuildPosition(size, out pos);
    }

    bool FindFreeBuildPosition(Vector2Int size, out Vector3 position)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 center = new Vector3(x, y, 0f);
                Vector2 boxSize = new Vector2(size.x, size.y);
                if (Physics2D.OverlapBox(center, boxSize, 0f) == null)
                {
                    position = center;
                    return true;
                }
            }
        }
        position = Vector3.zero;
        return false;
    }

    void AggregateStorage()
    {
        storages = FindObjectsOfType<StorageBuilding>().ToList();
        int w = 0, s = 0, f = 0;
        foreach (var st in storages)
        {
            if (st == null) continue;
            w += st.storedWood;
            s += st.storedStone;
            f += st.storedFood;
        }
        TotalWood = w;
        TotalStone = s;
        TotalFood = f;
    }

    public void NotifyResourceCollected(ResourceType type, int amount)
    {
        if (amount <= 0) return;
        switch (type)
        {
            case ResourceType.Wood: TotalWood += amount; break;
            case ResourceType.Stone: TotalStone += amount; break;
            case ResourceType.Food: TotalFood += amount; break;
        }
    }

    public StorageBuilding FindNearestStorage(Vector3 from)
    {
        StorageBuilding best = null;
        float bestDist = float.PositiveInfinity;
        foreach (var s in storages)
        {
            if (s == null) continue;
            float d = Vector3.Distance(from, s.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = s;
            }
        }
        return best;
    }

    void DebugActiveTasks()
    {
        if (activeTasks == null || activeTasks.Count == 0)
        {
            Debug.Log("[City] Aucune tâche active");
            return;
        }

        string log = "[City] Tâches actives: ";
        foreach (var t in activeTasks)
        {
            if (t == null || t.data == null) continue;
            string target = t.resourceTarget != null ? t.resourceTarget.name :
                            t.buildingData != null ? t.buildingData.buildingName : "N/A";
            log += $"[{t.data.taskName}:{t.data.type}->{target} assigned:{t.assignedVillagers.Count}] ";
        }
        Debug.Log(log);
    }

    void RefreshSceneListsIfNeeded()
    {
        var rn = FindObjectsOfType<ResourceNode>();
        if (rn.Length != resourceNodes.Count)
            resourceNodes = rn.ToList();

        var vs = FindObjectsOfType<VillagerUtilityAI>();
        if (vs.Length != villagers.Count)
            villagers = vs.ToList();

        var st = FindObjectsOfType<StorageBuilding>();
        if (st.Length != storages.Count)
            storages = st.ToList();
    }

    void RefreshSceneListsForce()
    {
        resourceNodes = FindObjectsOfType<ResourceNode>().ToList();
        villagers = FindObjectsOfType<VillagerUtilityAI>().ToList();
        storages = FindObjectsOfType<StorageBuilding>().ToList();
    }

    #endregion

    #region Worker calculation (AnimationCurve)

    public int ComputeRecommendedWorkers(CityTask task)
    {
        if (task == null || task.data == null) return 1;
        int population = Mathf.Max(1, villagers.Count);

        float normalizedPriority = Mathf.InverseLerp(0f, 10f, task.data.basePriority);

        float distanceFactor = 0f;
        if (task.resourceTarget != null)
        {
            float avgDist = villagers.Where(v => v != null).Average(v => Vector3.Distance(v.transform.position, task.resourceTarget.transform.position));
            distanceFactor = Mathf.Clamp01(avgDist / 30f);
        }

        float utilityScore = Mathf.Clamp01(normalizedPriority * 0.7f + distanceFactor * 0.3f);
        float percent = workerDistributionCurve.Evaluate(utilityScore);
        percent = Mathf.Clamp(percent, 0f, maxWorkerPercent);

        int recommended = Mathf.Max(1, Mathf.RoundToInt(population * percent));
        return recommended;
    }

    #endregion

    #region API publique

    public void RemoveTask(CityTask task)
    {
        if (task == null) return;
        if (activeTasks.Contains(task)) activeTasks.Remove(task);
    }

    #endregion
    private float[] CalculateAverages()
    {
        float totalHpNormalized = 0;
        float totalSpeedNormalized = 0;
        float totalStrengthNormalized = 0;

        foreach (var agent in villagers)
        {
            float hpNormalized = ((agent.Hp - agent.hpMin));
            float speedNormalized = ((agent.agent.speed - agent.speedMin)); 
            float strengthNormalized = ((agent.Strength - agent.strengthMin));

            totalHpNormalized += hpNormalized;
            totalSpeedNormalized += speedNormalized;
            totalStrengthNormalized += strengthNormalized;
        }

        float hpPercent = totalHpNormalized / ((villagers[0].hpMax - villagers[0].hpMin) * villagers.Count);
        float speedPercent = totalSpeedNormalized / ((villagers[0].speedMax - villagers[0].speedMin) * villagers.Count);
        float strengthPercent = totalStrengthNormalized / ((villagers[0].strengthMax - villagers[0].strengthMin) * villagers.Count);

        Debug.Log($"hpPercent : {hpPercent} ; speedPercent : {speedPercent} ; strengthPercent : {strengthPercent}");

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
        actionBasic?.Invoke(pExperienceReward);
    }

    public void AddDogmaSciencePoints(int pExperienceReward)
    {
        actionDogma?.Invoke(pExperienceReward);
    }
}
