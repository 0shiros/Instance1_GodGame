using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// CityUtilityAI
/// - Scanne la scène (villageois, nodes, storages)
/// - Génère des CityTask (collect, build)
/// - Assigne selon un score Utility
/// - Agrège les ressources depuis les StorageBuildings
/// - Calcule automatiquement le nombre recommandé de travailleurs via une AnimationCurve
/// - Empêche de construire au même endroit deux fois
/// - NotifyResourceCollected sécurisé pour ne pas générer d'erreur
/// - Placement des maisons aléatoire autour des maisons existantes
/// </summary>
public class CityUtilityAI : MonoBehaviour
{
    [Header("Datas & Références")]
    public List<TaskData> taskDataList = new List<TaskData>();
    public List<BuildingData> buildingTypes = new List<BuildingData>();

    [Header("Monde")]
    public Vector2Int gridSize = new Vector2Int(50, 50);
    public float taskScanInterval = 0.2f;

    [Header("Ressources globales")]
    public int totalWood;
    public int totalStone;
    public int totalFood;

    [Header("Distribution travailleurs")]
    public AnimationCurve workerDistributionCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);
    [Range(0f, 1f)]
    public float maxWorkerPercent = 0.5f;

    [HideInInspector] public List<VillagerUtilityAI> villagers = new List<VillagerUtilityAI>();
    private List<ResourceNode> resourceNodes = new List<ResourceNode>();
    private List<StorageBuilding> storages = new List<StorageBuilding>();
    public List<CityTask> activeTasks = new List<CityTask>();

    private List<Vector3> usedBuildPositions = new List<Vector3>();
    private List<Vector3> existingHouses = new List<Vector3>();
    private float timer = 0f;

    void Start()
    {
        RefreshSceneListsForce();
        AggregateStorage();
    }

    void Update()
    {
        timer += Time.deltaTime;
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
    }

    #region Tâches

    void CleanupFinishedTasks() => activeTasks.RemoveAll(t => t == null || t.isCompleted);

    void HandleResourceTasks()
    {
        foreach (var node in resourceNodes)
        {
            if (node == null || node.amount <= 0) continue;

            bool exists = activeTasks.Exists(t => t.data != null && t.data.type == TaskType.Collect && t.resourceTarget == node);
            if (exists) continue;

            var collectData = taskDataList.Find(td => td.type == TaskType.Collect && td.targetResource == node.resourceType);
            if (collectData == null) continue;

            var newTask = new CityTask { data = collectData, resourceTarget = node };
            activeTasks.Add(newTask);
        }
    }

    void HandleBuildTasks()
    {
        foreach (var building in buildingTypes)
        {
            if (building == null || building.prefab == null) continue;

            bool alreadyPlanned = activeTasks.Exists(t => t.data != null && t.data.type == TaskType.Build && t.buildingData == building);
            if (alreadyPlanned) continue;

            if (totalWood >= building.woodCost && totalStone >= building.stoneCost)
            {
                Vector3 buildPos = Vector3.zero;

                if (building.buildingType == BuildingType.House)
                {
                    // Position des maisons
                    if (existingHouses.Count == 0)
                    {
                        // première maison : trouve une position libre
                        if (!FindFreeBuildPosition(building.size, out buildPos)) continue;
                    }
                    else
                    {
                        // suivantes : position aléatoire autour d'une maison existante
                        int attempts = 10;
                        bool found = false;
                        for (int i = 0; i < attempts; i++)
                        {
                            Vector3 anchor = existingHouses[Random.Range(0, existingHouses.Count)];
                            float angle = Random.Range(0f, 360f);
                            float distance = Random.Range(3f, 6f); // distance min et max
                            Vector3 candidate = anchor + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f) * distance;

                            // vérifier que la position n'est pas déjà utilisée
                            if (!existingHouses.Any(p => Vector3.Distance(p, candidate) < 2f))
                            {
                                buildPos = candidate;
                                found = true;
                                break;
                            }
                        }
                        if (!found) continue;
                    }

                    existingHouses.Add(buildPos); // mémorise la position
                }
                else
                {
                    // autres bâtiments : position libre classique
                    if (!FindFreeBuildPosition(building.size, out buildPos)) continue;
                }

                var buildTaskData = taskDataList.Find(td => td.type == TaskType.Build && td.targetBuildingType == building.buildingType);
                if (buildTaskData == null) continue;

                CityTask t = new CityTask
                {
                    data = buildTaskData,
                    buildingData = building,
                    buildPosition = buildPos
                };
                activeTasks.Add(t);

                DeductResources(building.woodCost, building.stoneCost);
            }
        }
    }

    void DeductResources(int wood, int stone)
    {
        totalWood -= wood;
        totalStone -= stone;

        foreach (var st in storages)
        {
            if (wood > 0)
            {
                int taken = st.Withdraw(ResourceType.Wood, wood);
                wood -= taken;
            }
            if (stone > 0)
            {
                int taken = st.Withdraw(ResourceType.Stone, stone);
                stone -= taken;
            }
            if (wood <= 0 && stone <= 0) break;
        }
    }

    #endregion

    #region Assignation

    void AssignTasksToVillagers()
    {
        foreach (var villager in villagers)
        {
            if (villager == null || !villager.IsIdle()) continue;
            AssignTaskToVillager(villager);
        }
    }

    public void AssignTaskToVillager(VillagerUtilityAI villager, CityTask forced = null)
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

        if (forced != null && !forced.isCompleted) best = forced;

        if (best != null)
        {
            if (!best.assignedVillagers.Contains(villager))
                best.assignedVillagers.Add(villager);
            villager.AssignTask(best);
        }
    }

    float ScoreTaskForVillager(CityTask task, VillagerUtilityAI villager)
    {
        float score = (task.data != null) ? task.data.basePriority : 0f;
        if (task.data == null) return score;

        if (task.data.type == TaskType.Collect && task.resourceTarget != null)
        {
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

        score += Random.Range(-0.5f, 0.5f);
        return score;
    }

    #endregion

    #region Helpers

    bool FindFreeBuildPosition(Vector2Int size, out Vector3 position)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 center = new Vector3(x, y, 0f);
                Vector2 boxSize = new Vector2(size.x, size.y);

                bool free = Physics2D.OverlapBox(center, boxSize, 0f) == null;
                bool used = usedBuildPositions.Contains(center);

                if (free && !used)
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
        totalWood = storages.Sum(s => s.storedWood);
        totalStone = storages.Sum(s => s.storedStone);
        totalFood = storages.Sum(s => s.storedFood);
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

    void RefreshSceneListsIfNeeded()
    {
        var rn = FindObjectsOfType<ResourceNode>();
        if (rn.Length != resourceNodes.Count) resourceNodes = rn.ToList();

        var vs = FindObjectsOfType<VillagerUtilityAI>();
        if (vs.Length != villagers.Count) villagers = vs.ToList();

        var st = FindObjectsOfType<StorageBuilding>();
        if (st.Length != storages.Count) storages = st.ToList();
    }

    void RefreshSceneListsForce()
    {
        resourceNodes = FindObjectsOfType<ResourceNode>().ToList();
        villagers = FindObjectsOfType<VillagerUtilityAI>().ToList();
        storages = FindObjectsOfType<StorageBuilding>().ToList();
    }

    #endregion

    #region Worker Calculation

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

        return Mathf.Max(1, Mathf.RoundToInt(population * percent));
    }

    #endregion

    #region API Publique

    public void RemoveTask(CityTask task)
    {
        if (task == null) return;
        if (activeTasks.Contains(task)) activeTasks.Remove(task);
    }

    /// <summary>
    /// Méthode sécurisée pour notifier la collecte d'une ressource
    /// </summary>
    public void NotifyResourceCollected(ResourceType type, int amount)
    {
        if (amount <= 0) return;

        switch (type)
        {
            case ResourceType.Wood:
                totalWood += amount;
                break;
            case ResourceType.Stone:
                totalStone += amount;
                break;
            case ResourceType.Food:
                totalFood += amount;
                break;
        }
    }

    #endregion
}
