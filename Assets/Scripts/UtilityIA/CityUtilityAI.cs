// CityUtilityAI.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Manager central de la cité.
/// - Scanne la scène (villagers, resource nodes, storages)
/// - Génère des tâches (collect, build, deposit)
/// - Assigne les tâches selon un Utility Score simple
/// - Maintient des ressources globales (optionnel)
/// </summary>
public class CityUtilityAI : MonoBehaviour
{
    [Header("Datas & Références")]
    public List<TaskData> taskDataList = new List<TaskData>();
    public List<BuildingData> buildingTypes = new List<BuildingData>();

    [Header("Monde")]
    public Vector2Int gridSize = new Vector2Int(50, 50);
    public float taskScanInterval = 1f; // toutes les X secondes on (re)calcule les tâches/priorités

    [Header("Ressources globales (agrégées des dépôts)")]
    public int totalWood;
    public int totalStone;
    public int totalFood;

    // internes
    [HideInInspector] public List<VillagerUtilityAI> villagers = new List<VillagerUtilityAI>();
    private List<ResourceNode> resourceNodes = new List<ResourceNode>();
    private List<StorageBuilding> storages = new List<StorageBuilding>();
    private List<CityTask> activeTasks = new List<CityTask>();

    private float timer = 0f;

    void Start()
    {
        // découverte initiale
        villagers = FindObjectsOfType<VillagerUtilityAI>().ToList();
        resourceNodes = FindObjectsOfType<ResourceNode>().ToList();
        storages = FindObjectsOfType<StorageBuilding>().ToList();

        // optionnel : agréger les stocks initiaux
        AggregateStorage();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= taskScanInterval)
        {
            timer = 0f;
            CleanupFinishedTasks();
            HandleResourceTasks();
            HandleBuildTasks();
            HandleDepositTasks();
            AssignTasksToVillagers();
            AggregateStorage();
        }
    }

    #region Tâches / Création
    void CleanupFinishedTasks()
    {
        // supprime les tâches complétées (sécurité)
        activeTasks.RemoveAll(t => t == null || t.isCompleted);
    }

    void HandleResourceTasks()
    {
        // pour chaque nœud de ressource encore plein, créer une tâche Collect si pas déjà existante
        foreach (var node in resourceNodes)
        {
            if (node == null || node.amount <= 0) continue;
            bool exists = activeTasks.Exists(t => t.data != null && t.data.type == TaskType.Collect && t.resourceTarget == node);
            if (!exists)
            {
                var collectData = taskDataList.Find(td => td.type == TaskType.Collect && td.targetResource == node.resourceType);
                if (collectData != null)
                {
                    CityTask newT = new CityTask
                    {
                        data = collectData,
                        resourceTarget = node
                    };
                    activeTasks.Add(newT);
                }
            }
        }
    }

    void HandleBuildTasks()
    {
        // vérifie les bâtiments définis et crée une tâche build si ressources globales suffisantes et pas déjà planifiée
        foreach (var building in buildingTypes)
        {
            // Skip si prefab manquant
            if (building.prefab == null) continue;

            bool alreadyPlanned = activeTasks.Exists(t => t.data != null && t.data.type == TaskType.Build && t.buildingData == building);
            if (alreadyPlanned) continue;

            // TODO: logique de décision pour construire (ex: nombre de maisons)
            // Exemple simple: si on a assez de ressources, on planifie une construction
            if (totalWood >= building.woodCost && totalStone >= building.stoneCost)
            {
                // trouve une position libre (simple brute-force)
                if (FindFreeBuildPosition(building.size, out Vector3 pos))
                {
                    var buildTaskData = taskDataList.Find(td => td.type == TaskType.Build && td.targetBuildingType == building.buildingType);
                    if (buildTaskData != null)
                    {
                        CityTask t = new CityTask
                        {
                            data = buildTaskData,
                            buildingData = building,
                            buildPosition = pos
                        };
                        activeTasks.Add(t);

                        // Consommer immédiatement les ressources globales (réservation)
                        totalWood -= building.woodCost;
                        totalStone -= building.stoneCost;
                    }
                }
            }
        }
    }

    void HandleDepositTasks()
    {
        // S'il n'y a pas de dépôt planifié mais qu'il existe des ressources en wpocket (ex: heuristique)
        // Ici on ne crée pas de tâches Deposit automatiques, mais on en crée si il existe des villagers
        // qui portent des ressources et aucun depositTarget assigné. (Simplification: City ne tracke pas l'inventaire individuel)
    }
    #endregion

    #region Assignation
    void AssignTasksToVillagers()
    {
        // Pour chaque villageois libre, calcule la meilleure tâche par scoring utility
        foreach (var villager in villagers)
        {
            if (villager == null) continue;
            if (!villager.IsIdle()) continue;

            CityTask best = null;
            float bestScore = float.NegativeInfinity;

            foreach (var task in activeTasks)
            {
                if (task == null || task.isCompleted) continue;

                // si la tâche a déjà autant de workers que recommandé, on peut la considérer pleine
                if (task.assignedVillagers.Count >= (task.data != null ? task.data.recommendedVillagers : 1)) continue;

                float score = ScoreTaskForVillager(task, villager);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = task;
                }
            }

            if (best != null)
            {
                best.assignedVillagers.Add(villager);
                villager.AssignTask(best);
            }
        }
    }

    float ScoreTaskForVillager(CityTask task, VillagerUtilityAI villager)
    {
        // Score de base = basePriority
        float score = (task.data != null) ? task.data.basePriority : 0f;

        // Bonus selon type et besoins de la cité
        if (task.data.type == TaskType.Collect && task.resourceTarget != null)
        {
            // + si la ressource est critique (peu de stock)
            if (task.resourceTarget.resourceType == ResourceType.Wood && totalWood < 5) score += 10f;
            if (task.resourceTarget.resourceType == ResourceType.Stone && totalStone < 3) score += 8f;

            // - distance (préférence pour proche) : calcul simple
            float dist = Vector3.Distance(villager.transform.position, task.resourceTarget.transform.position);
            score -= dist * 0.1f;
        }
        else if (task.data.type == TaskType.Build && task.buildingData != null)
        {
            // Priorité selon le type de bâtiment (logement prioritaire si population > cap)
            if (task.buildingData.buildingType == BuildingType.House)
            {
                int population = villagers.Count;
                int houses = GameObject.FindGameObjectsWithTag("Building").Length; // simplification
                float need = population / (float)task.buildingData.housingCapacity;
                score += Mathf.Max(0f, need - houses) * 2f;
            }
            // - distance vers le site
            float dist = Vector3.Distance(villager.transform.position, task.buildPosition);
            score -= dist * 0.05f;
        }

        // Bonus si le rôle du villageois correspond
        if (villager.role == VillagerRole.Gatherer && task.data.type == TaskType.Collect) score += 5f;
        if (villager.role == VillagerRole.Builder && task.data.type == TaskType.Build) score += 5f;

        // Petite variance aléatoire pour éviter ties
        score += Random.Range(-0.5f, 0.5f);

        return score;
    }
    #endregion

    #region Helpers
    // Recherche d'une position libre simple (peut être améliorée)
    bool FindFreeBuildPosition(Vector2Int size, out Vector3 position)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 center = new Vector3(x, y, 0f);
                if (Physics2D.OverlapBox(center, size, 0f) == null)
                {
                    position = center;
                    return true;
                }
            }
        }
        position = Vector3.zero;
        return false;
    }

    // Aggrégation des stocks depuis les StorageBuildings présents
    void AggregateStorage()
    {
        int w = 0, s = 0, f = 0;
        foreach (var st in storages)
        {
            if (st == null) continue;
            w += st.storedWood;
            s += st.storedStone;
            f += st.storedFood;
        }
        totalWood = w;
        totalStone = s;
        totalFood = f;
    }

    // Fonction publique pour qu'un villageois notifie une collecte/dépôt
    public void NotifyResourceCollected(ResourceType type, int amount)
    {
        // Si on veut, on peut temporairement garder un "pocket" avant dépôt
        // Ici on agrège directement (mais le mieux est que les villagers déposent dans un Storage)
        if (type == ResourceType.Wood) totalWood += amount;
        if (type == ResourceType.Stone) totalStone += amount;
        if (type == ResourceType.Food) totalFood += amount;
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
    #endregion
}
