
// CityUtilityAI.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CityUtilityAI : MonoBehaviour
{
    [Header("Datas & Références")]
    public List<TaskData> TaskDataList = new List<TaskData>();
    public List<BuildingData> BuildingTypes = new List<BuildingData>();

    [Header("Grid")]
    public GridManager2D GridManager;

    [Header("Monde")]
    public Vector2Int GridSize = new Vector2Int(50, 50);
    public float TaskScanInterval = 0.2f; // secondes

    [Header("Placement bâtiments")]
    public float HouseSpawnDistance = 5f;
    public float BuildingMinDistance = 2f;

    [Header("Ressources globales")]
    public int TotalWood;
    public int TotalStone;
    public int TotalFood;

    [Header("Réglages distribution travailleurs")]
    public AnimationCurve WorkerDistributionCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);
    [Range(0f, 1f)]
    public float MaxWorkerPercent = 0.5f;

    // internes
    [HideInInspector] public List<VillagerUtilityAI> villagers = new List<VillagerUtilityAI>();
    private List<ResourceNode> resourceNodes = new List<ResourceNode>();
    private List<StorageBuilding> storages = new List<StorageBuilding>();
    public List<CityTask> ActiveTasks = new List<CityTask>();

    private float timer = 0f;
    private float debugTimer = 0f;

    public static Action<int> ActionBasic;
    public static Action<int> ActionDogma;
    public E_Dogma CurrentDogma = E_Dogma.None;
    public int AgentsQuantity;
    [SerializeField] private int agentsQuantityNeedToSetDogma;


    // --- START / UPDATE
    void Start()
    {
        if (GridManager == null)
        {
            GridManager = FindObjectOfType<GridManager2D>();
            if (GridManager == null)
                Debug.LogWarning("[City] GridManager2D not found in scene. Building placement will fail.");
        }

        RefreshSceneListsForce();
        AggregateStorage();

        // ne touche pas au bloc CalculateAverages/SetDogma/AddSciencePoints
        SetDogma();
        AddSciencePoints(6);
        AddDogmaSciencePoints(4);
    }

    void Update()
    {
        timer += Time.deltaTime;
        debugTimer += Time.deltaTime;

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
            DebugActiveTasks();
        }
    }

    #region Tâches / Création

    void CleanupFinishedTasks()
    {
        ActiveTasks.RemoveAll(t => t == null || t.IsCompleted);
    }

    void HandleResourceTasks()
    {
        foreach (var node in resourceNodes)
        {
            if (node == null || node.Amount <= 0) continue;
            bool exists = ActiveTasks.Exists(t => t.Data != null && t.Data.Type == TaskType.Collect && t.ResourceTarget == node);
            if (exists) continue;

            var collectData = TaskDataList.Find(td => td.Type == TaskType.Collect && td.TargetResource == node.ResourceType);
            if (collectData == null) continue;

            var newT = new CityTask { Data = collectData, ResourceTarget = node };
            ActiveTasks.Add(newT);
        }
    }

    void HandleBuildTasks()
    {
        if (GridManager == null) return;

        foreach (var building in BuildingTypes)
        {
            if (building == null || building.Prefab == null) continue;

            bool alreadyPlanned = ActiveTasks.Exists(t => t.Data != null && t.Data.Type == TaskType.Build && t.BuildingData == building);
            if (alreadyPlanned) continue;

            if (TotalWood >= building.WoodCost && TotalStone >= building.StoneCost)
            {
                Vector2Int targetCell;

                // --- PREMIER BÂTIMENT : placer dans la cellule LIBRE LA PLUS PROCHE DU CITYAI ---
                if (GridManager.GetCellsOwnedByCity(this).Count == 0)
                {
                    if (!GridManager.TryFindNearestFreeCell(transform.position, building.Size, out targetCell))
                    {
                        Debug.LogWarning("[City] Impossible de trouver une cellule proche pour le premier bâtiment.");
                        continue;
                    }
                }
                else
                {
                    // --- Construction normale → cell adjacente prioritaire ---
                    if (!GridManager.TryFindCellAdjacentToCity(this, building, out targetCell))
                    {
                        if (!GridManager.TryFindRandomCellAroundHouse(building.Size, out targetCell))
                            continue;
                    }
                }

                // Réserver la cellule
                if (GridManager.TryReserveCell(this, targetCell, building, out Vector3 reservedWorldPos))
                {
                    // Placement aléatoire dans la cellule
                    reservedWorldPos += new Vector3(
                        UnityEngine.Random.Range(-0.4f, 0.4f) * GridManager.CellSize,
                        0,
                        UnityEngine.Random.Range(-0.4f, 0.4f) * GridManager.CellSize
                    );

                    TotalWood -= building.WoodCost;
                    TotalStone -= building.StoneCost;

                    var buildTaskData = TaskDataList.Find(td => td.Type == TaskType.Build && td.TargetBuildingType == building.BuildingType);
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

        if (pForced != null && !pForced.IsCompleted) best = pForced;

        if (best != null)
        {
            if (!best.AassignedVillagers.Contains(pVillager))
                best.AassignedVillagers.Add(pVillager);

            pVillager.AssignTask(best);
            Debug.Log($"[City] AssignTask: {pVillager.name} -> {best.Data.TaskName} ({best.Data.Type})");
        }
    }

    float ScoreTaskForVillager(CityTask pRask, VillagerUtilityAI pVillager)
    {
        float score = (pRask.Data != null) ? pRask.Data.BasePriority : 0f;
        if (pRask.Data == null) return score;

        if (pRask.Data.Type == TaskType.Collect && pRask.ResourceTarget != null)
        {
            if (pRask.ResourceTarget.ResourceType == ResourceType.Wood && TotalWood < 5) score += 10f;
            if (pRask.ResourceTarget.ResourceType == ResourceType.Stone && TotalStone < 3) score += 8f;

            float dist = Vector3.Distance(pVillager.transform.position, pRask.ResourceTarget.transform.position);
            score -= dist * 0.1f;
        }
        else if (pRask.Data.Type == TaskType.Build && pRask.BuildingData != null)
        {
            float dist = Vector3.Distance(pVillager.transform.position, pRask.BuildPosition);
            score -= dist * 0.05f;
        }

        if (pVillager.role == VillagerRole.Gatherer && pRask.Data.Type == TaskType.Collect) score += 5f;
        if (pVillager.role == VillagerRole.Builder && pRask.Data.Type == TaskType.Build) score += 5f;

        score += UnityEngine.Random.Range(-0.5f, 0.5f);
        return score;
    }

    #endregion

    #region Helpers

    void TryRecruitNearbyVillagers()
    {
        Vector3 cityCenter = transform.position;
        foreach (var v in FindObjectsOfType<VillagerUtilityAI>())
        {
            if (villagers.Contains(v)) continue;
            float d = Vector3.Distance(v.transform.position, cityCenter);
            if (d < 15f)
            {
                villagers.Add(v);
                Debug.Log($"[City] Nouveau villageois rejoint le village : {v.name}");
            }
        }
    }

    void AggregateStorage()
    {
        storages = FindObjectsOfType<StorageBuilding>().ToList();
        int w = 0, s = 0, f = 0;
        foreach (var st in storages)
        {
            if (st == null) continue;
            w += st.StoredWood;
            s += st.StoredStone;
            f += st.StoredFood;
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

    void DebugActiveTasks()
    {
        if (ActiveTasks == null || ActiveTasks.Count == 0)
        {
            Debug.Log("[City] Aucune tâche active");
            return;
        }

        string log = "[City] Tâches actives: ";
        foreach (var t in ActiveTasks)
        {
            if (t == null || t.Data == null) continue;
            string target = t.ResourceTarget != null ? t.ResourceTarget.name :
                            t.BuildingData != null ? t.BuildingData.BuildingName : "N/A";
            log += $"[{t.Data.TaskName}:{t.Data.Type}->{target} assigned:{t.AassignedVillagers.Count}] ";
        }
        Debug.Log(log);
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

    #region Worker calculation (AnimationCurve)

    public int ComputeRecommendedWorkers(CityTask pTask)
    {
        if (pTask == null || pTask.Data == null) return 1;
        int population = Mathf.Max(1, villagers.Count);

        float normalizedPriority = Mathf.InverseLerp(0f, 10f, pTask.Data.BasePriority);

        float distanceFactor = 0f;
        if (pTask.ResourceTarget != null)
        {
            float avgDist = villagers.Where(v => v != null).Average(v => Vector3.Distance(v.transform.position, pTask.ResourceTarget.transform.position));
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
        if (ActiveTasks.Contains(pTask)) ActiveTasks.Remove(pTask);
    }

    #endregion

    #region ----- NE PAS MODIFIER : CalculateAverages / SetDogma / AddSciencePoints -----
    // Tu as demandé que cette partie ne soit jamais modifiée — je la laisse intacte.

    private float[] CalculateAverages()
    {
        float totalHpNormalized = 0;
        float totalSpeedNormalized = 0;
        float totalStrengthNormalized = 0;

        foreach (var agent in villagers)
        {
            float hpNormalized = ((agent.Hp - agent.HpMin));
            float speedNormalized = ((agent.agent.speed - agent.SpeedMin));
            float strengthNormalized = ((agent.Strength - agent.StrengthMin));

            totalHpNormalized += hpNormalized;
            totalSpeedNormalized += speedNormalized;
            totalStrengthNormalized += strengthNormalized;
        }

        float hpPercent = totalHpNormalized / ((villagers[0].HpMax - villagers[0].HpMin) * villagers.Count);
        float speedPercent = totalSpeedNormalized / ((villagers[0].SpeedMax - villagers[0].SpeedMin) * villagers.Count);
        float strengthPercent = totalStrengthNormalized / ((villagers[0].StrengthMax - villagers[0].StrengthMin) * villagers.Count);

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
        ActionBasic?.Invoke(pExperienceReward);
    }

    public void AddDogmaSciencePoints(int pExperienceReward)
    {
        ActionDogma?.Invoke(pExperienceReward);
    }
    #endregion

}
