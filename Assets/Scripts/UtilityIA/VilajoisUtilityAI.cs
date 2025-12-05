using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// VillagerUtilityAI - version finale corrigée
/// - State machine légère via enum interne
/// - Coroutines cancelables
/// - Compatible avec TaskData.workDuration
/// - Utilise CityUtilityAI.NotifyResourceCollected/FindNearestStorage/RemoveTask
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class VillagerUtilityAI : MonoBehaviour
{
    public VillagerRole role = VillagerRole.Generalist;

    [Header("Capacités")]
    public int carryCapacity = 5;
    public int harvestPerAction = 2;

    [Header("Besoins")]
    public float initialHunger = 0f;
    public float initialFatigue = 0f;
    public float hungerRate = 0.5f;
    public float fatigueRate = 0.2f;
    public float hungerThreshold = 80f;
    public float fatigueThreshold = 90f;

    [Header("Mouvement")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 0.2f;

    [Header("Références")]
    public CityUtilityAI city;

    [Header("Construction fallback")]
    [Tooltip("Durée par défaut de construction si TaskData.workDuration est 0.")]
    public float defaultBuildTime = 2f;

    private NavMeshAgent agent;
    private enum EState { Idle, Moving, Working, Depositing, Eating, Sleeping }
    private EState state = EState.Idle;

    // Tâche et inventaire
    private CityTask currentTask;
    public int carrying = 0;
    public ResourceType carryingType = ResourceType.None;

    public float hunger;
    public float fatigue;

    private Coroutine actionCoroutine;
    private Animator animator;

    public bool isBusy => state != EState.Idle;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;
        }
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        hunger = initialHunger;
        fatigue = initialFatigue;
        if (city == null) city = FindObjectOfType<CityUtilityAI>();
    }

    void Update()
    {
        hunger += hungerRate * Time.deltaTime;
        fatigue += fatigueRate * Time.deltaTime;

        if (hunger >= hungerThreshold)
        {
            if (!HasImmediateFoodGoal()) TryCreateImmediateFoodTask();
        }

        UpdateAnimator();
    }

    #region Assign / Abandon

    public void AssignTask(CityTask task)
    {
        if (task == null) return;
        if (currentTask == task) return;

        StopCurrentAction();
        currentTask = task;

        switch (task.data?.type)
        {
            case TaskType.Collect:
                StartCollectFlow(task);
                break;
            case TaskType.Build:
                StartBuildFlow(task);
                break;
            default:
                StartIdle();
                break;
        }
    }

    public void AbandonCurrentTask()
    {
        if (currentTask != null)
        {
            currentTask.assignedVillagers.Remove(this);
            currentTask = null;
        }
        StopCurrentAction();
        StartIdle();
    }

    public bool IsIdle()
    {
        return !isBusy && currentTask == null;
    }

    #endregion

    #region Collect / Build flows

    private void StartCollectFlow(CityTask task)
    {
        if (task == null || task.resourceTarget == null)
        {
            FinishCurrentTask();
            return;
        }
        actionCoroutine = StartCoroutine(CollectRoutine(task));
    }

    private IEnumerator CollectRoutine(CityTask task)
    {
        state = EState.Moving;
        if (!GoToPosition(task.resourceTarget.transform.position))
        {
            FinishCurrentTask();
            yield break;
        }
        yield return StartCoroutine(WaitUntilArrived());

        state = EState.Working;
        while (task.resourceTarget != null && task.resourceTarget.amount > 0)
        {
            int canTake = Mathf.Min(carryCapacity - carrying, harvestPerAction, task.resourceTarget.amount);
            if (canTake <= 0)
            {
                yield return StartCoroutine(HandleDepositFlow());
                if (carrying >= carryCapacity) break;
            }
            else
            {
                task.resourceTarget.amount -= canTake;
                carrying += canTake;
                carryingType = task.resourceTarget.resourceType;

                fatigue += 1f;
                hunger += 0.5f;

                if (carrying >= carryCapacity)
                {
                    yield return StartCoroutine(HandleDepositFlow());
                }
                else
                {
                    if (task.resourceTarget.amount <= 0) break;
                    yield return new WaitForSeconds(0.3f);
                }
            }

            if (task.isCompleted) break;
            if (currentTask != task) yield break;
        }

        FinishCurrentTask();
    }

    private void StartBuildFlow(CityTask task)
    {
        if (task == null || task.buildingData == null)
        {
            FinishCurrentTask();
            return;
        }
        actionCoroutine = StartCoroutine(BuildRoutine(task));
    }

    private IEnumerator BuildRoutine(CityTask task)
    {
        state = EState.Moving;
        if (!GoToPosition(task.buildPosition))
        {
            FinishCurrentTask();
            yield break;
        }
        yield return StartCoroutine(WaitUntilArrived());

        state = EState.Working;

        float buildTime = (task.data != null && task.data.workDuration > 0f) ? task.data.workDuration : defaultBuildTime;
        float elapsed = 0f;
        while (elapsed < buildTime)
        {
            if (currentTask != task) yield break;
            elapsed += Time.deltaTime;
            fatigue += 0.1f * Time.deltaTime;
            hunger += 0.05f * Time.deltaTime;
            yield return null;
        }

        if (task.buildingData != null && task.buildingData.prefab != null)
            Instantiate(task.buildingData.prefab, task.buildPosition, Quaternion.identity);

        FinishCurrentTask();
    }

    private IEnumerator HandleDepositFlow()
    {
        state = EState.Depositing;

        // Stockage le plus proche
        StorageBuilding storage = city?.FindNearestStorage(transform.position);

        if (storage == null)
        {
            // Pas de stockage mais ressources à remettre
            if (carrying > 0 && city != null)
            {
                city.NotifyResourceCollected(carryingType, carrying);
                carrying = 0;
                carryingType = ResourceType.None;
            }
            yield break;
        }

        if (!GoToPosition(storage.transform.position)) yield break;
        yield return StartCoroutine(WaitUntilArrived());

        if (carrying > 0)
        {
            storage.Deposit(carryingType, carrying);
            city.NotifyResourceCollected(carryingType, carrying);
            carrying = 0;
            carryingType = ResourceType.None;
        }

        yield return new WaitForSeconds(0.1f);
    }

    #endregion

    #region Movement helpers

    private bool GoToPosition(Vector3 pos)
    {
        if (agent == null) return false;
        agent.isStopped = false;
        agent.SetDestination(pos);
        return true;
    }

    private IEnumerator WaitUntilArrived()
    {
        if (agent == null) yield break;
        float timeout = 10f;
        float t = 0f;
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            if (currentTask == null) yield break;
            t += Time.deltaTime;
            if (t > timeout) yield break;
            yield return null;
        }
    }

    private void StopCurrentAction()
    {
        if (actionCoroutine != null)
        {
            StopCoroutine(actionCoroutine);
            actionCoroutine = null;
        }
        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
        state = EState.Idle;
    }

    #endregion

    #region Finish task & helpers

    private void FinishCurrentTask()
    {
        if (currentTask == null)
        {
            StartIdle();
            return;
        }

        var done = currentTask;
        currentTask = null;

        done.isCompleted = true;
        if (done.assignedVillagers.Contains(this)) done.assignedVillagers.Remove(this);

        city?.RemoveTask(done);

        StopCurrentAction();
        StartIdle();
    }

    private void StartIdle()
    {
        state = EState.Idle;
    }

    #endregion

    #region Food urgent helpers

    private bool HasImmediateFoodGoal()
    {
        if (currentTask == null) return false;
        return currentTask.data != null &&
               currentTask.data.type == TaskType.Collect &&
               currentTask.resourceTarget != null &&
               currentTask.resourceTarget.resourceType == ResourceType.Food;
    }

    private void TryCreateImmediateFoodTask()
    {
        if (city == null) return;
        ResourceNode foodNode = FindNearestResource(ResourceType.Food);
        if (foodNode == null) return;

        var td = city.taskDataList?.Find(t => t.type == TaskType.Collect && t.targetResource == ResourceType.Food);
        if (td == null) return;

        CityTask temp = new CityTask { data = td, resourceTarget = foodNode };
        AssignTask(temp);
    }

    private ResourceNode FindNearestResource(ResourceType type)
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

    #endregion

    #region Anim & Debug

    private void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetBool("isMoving", state == EState.Moving);
        animator.SetBool("isWorking", state == EState.Working);
        animator.SetBool("isDepositing", state == EState.Depositing);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position + Vector3.up * 0.2f, 0.05f);

#if UNITY_EDITOR
        if (Application.isPlaying)
            Handles.Label(transform.position + Vector3.up * 0.35f, $"State: {state}\nCarrying: {carrying}");
#endif
    }

    #endregion
}
