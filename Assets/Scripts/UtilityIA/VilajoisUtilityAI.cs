// VillagerUtilityAI.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// IA du villageois (2D). Comportements :
/// - Idle -> accepte une task (AssignTask)
/// - Collect : va au ResourceNode, prend harvestPerAction unités, puis cherche storage, dépose
/// - Build : va à buildPosition et instancie le prefab (relié au CityUtilityAI)
/// - Gestion faim/fatigue (simplifiée)
/// 
/// NOTE: Si tu utilises NavMesh+ pour 2D, remplace NavMeshAgent par l'agent 2D correspondant.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class VillagerUtilityAI : MonoBehaviour
{
    [Header("Role & capacités")]
    public VillagerRole role = VillagerRole.Generalist;
    public int carryCapacity = 1;      // combien d'unités peut porter
    public int carrying = 0;           // actuellement porté
    public ResourceType carryingType = ResourceType.None;

    [Header("Besoins")]
    public float hunger = 0f;
    public float fatigue = 0f;
    public float hungerRate = 1f;
    public float fatigueRate = 0.5f;
    public float hungerThreshold = 50f;
    public float fatigueThreshold = 60f;

    [Header("Référence")]
    public CityUtilityAI city;

    private NavMeshAgent agent; // Remplacer si besoin selon NavMesh2D
    private CityTask currentTask;
    public bool isBusy = false;

    private enum State { Idle, Moving, Working, Eating, Sleeping, Depositing }
    private State state = State.Idle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        if (city == null)
            city = FindObjectOfType<CityUtilityAI>();
    }

    void Update()
    {
        // Update besoins
        hunger += hungerRate * Time.deltaTime;
        fatigue += fatigueRate * Time.deltaTime;

        // Si besoins critiques -> ignorer tâches, aller manger/dormir (si tu as Food/Bed)
        if (hunger >= hungerThreshold)
        {
            // priorité manger : si dépôt ou ressource Food existante
            var foodNode = FindNearestResource(ResourceType.Food);
            if (foodNode != null)
            {
                // créer une tâche temporaire locale ou aller manger directement
                // Pour simplicité : on va collecter comme une ressource
                if (currentTask == null || currentTask.data.type != TaskType.Collect)
                {
                    var temp = new CityTask { data = city.taskDataList.Find(t => t.type == TaskType.Collect && t.targetResource == ResourceType.Food), resourceTarget = foodNode };
                    AssignTask(temp);
                }
                return;
            }
        }

        // Si on a une tâche
        if (currentTask != null && !currentTask.isCompleted)
        {
            HandleCurrentTask();
        }
        else
        {
            // état idle
            state = State.Idle;
            isBusy = false;
        }
    }

    void HandleCurrentTask()
    {
        if (currentTask.data.type == TaskType.Collect && currentTask.resourceTarget != null)
        {
            isBusy = true;
            // se déplacer vers la ressource
            SetDestination(currentTask.resourceTarget.transform.position);
            state = State.Moving;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // collecte
                CollectFromNode(currentTask.resourceTarget);
            }
        }
        else if (currentTask.data.type == TaskType.Build && currentTask.buildingData != null)
        {
            isBusy = true;
            SetDestination(currentTask.buildPosition);
            state = State.Moving;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // effectue la construction (simple : instancier le prefab)
                BuildAtPosition(currentTask);
            }
        }
    }

    void SetDestination(Vector3 pos)
    {
        if (agent == null) return;
        if (agent.destination != pos) agent.SetDestination(pos);
    }

    void CollectFromNode(ResourceNode node)
    {
        if (node.amount <= 0)
        {
            // tâche terminée (plus rien à collecter)
            FinishCurrentTask();
            return;
        }

        int canTake = Mathf.Min(carryCapacity - carrying, node.harvestPerAction, node.amount);
        if (canTake <= 0)
        {
            // si on ne peut rien prendre -> chercher dépôt si on a du transport
            if (carrying > 0)
            {
                GoDeposit();
                return;
            }
            else
            {
                FinishCurrentTask();
                return;
            }
        }

        // action de collecte : décrémenter node, incrémenter inventory local
        node.amount -= canTake;
        carrying += canTake;
        carryingType = node.resourceType;

        // notifier la cité (temporaire) — la vraie agrégation se fera au dépôt
        // city.NotifyResourceCollected(node.resourceType, canTake); // facultatif

        // si on est plein -> aller déposer
        if (carrying >= carryCapacity)
        {
            GoDeposit();
        }
        else
        {
            // si node vide -> finish
            if (node.amount <= 0)
                FinishCurrentTask();
        }
    }

    void GoDeposit()
    {
        // trouver stockage le plus proche
        var storage = city.FindNearestStorage(transform.position);
        if (storage == null)
        {
            // pas de stockage => notifier la cité directement
            city.NotifyResourceCollected(carryingType, carrying);
            carrying = 0;
            carryingType = ResourceType.None;
            FinishCurrentTask(); // ou rester idle
            return;
        }

        // créer une tâche de dépôt locale (ou déplacer vers storage directement)
        SetDestination(storage.transform.position);
        state = State.Depositing;

        // quand arrivé -> déposer
        StartCoroutine(WaitAndDeposit(storage));
    }

    IEnumerator WaitAndDeposit(StorageBuilding storage)
    {
        // attend l'arrivée (polling)
        while (agent != null && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
            yield return null;

        // dépôt réel
        if (carrying > 0)
        {
            storage.Deposit(carryingType, carrying);
            // notifier la cité
            city.NotifyResourceCollected(carryingType, carrying);
            carrying = 0;
            carryingType = ResourceType.None;
        }

        // Fin de tâche / retour Idle
        FinishCurrentTask();
    }

    void BuildAtPosition(CityTask task)
    {
        if (task.buildingData.prefab == null)
        {
            FinishCurrentTask();
            return;
        }

        // instanciation du prefab (on suppose que ressources ont été réservées côté CityUtilityAI)
        GameObject.Instantiate(task.buildingData.prefab, task.buildPosition, Quaternion.identity);

        // marque la tâche comme complétée
        FinishCurrentTask();
    }

    void FinishCurrentTask()
    {
        if (currentTask != null)
        {
            // notifier le city manager
            CityTask done = currentTask;
            currentTask = null;
            state = State.Idle;
            isBusy = false;

            if (done != null)
            {
                done.isCompleted = true;
                done.assignedVillagers.Remove(this);
                // notifications additionnelles peuvent être appelées ici
            }
        }
    }

    public void AssignTask(CityTask task)
    {
        // reçoit une tâche du CityUtilityAI
        currentTask = task;
        if (task != null && !task.assignedVillagers.Contains(this))
            task.assignedVillagers.Add(this);

        isBusy = true;
    }

    public bool IsIdle()
    {
        return !isBusy && currentTask == null;
    }

    ResourceNode FindNearestResource(ResourceType type)
    {
        ResourceNode best = null;
        float bestDist = float.PositiveInfinity;
        foreach (var rn in FindObjectsOfType<ResourceNode>())
        {
            if (rn == null || rn.resourceType != type || rn.amount <= 0) continue;
            float d = Vector3.Distance(transform.position, rn.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = rn;
            }
        }
        return best;
    }
}
