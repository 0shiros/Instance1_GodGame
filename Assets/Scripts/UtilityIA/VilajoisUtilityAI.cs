using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class VillagerUtilityAI : MonoBehaviour
{
    public VillagerRole role = VillagerRole.Generalist;

    [Header("Capacités")]
    public int carryCapacity = 5;
    public int harvestPerAction = 2;

    [Header("Besoins")]
    public float hunger = 0f;
    public float fatigue = 0f;
    public float hungerRate = 0.5f;
    public float fatigueRate = 0.2f;
    public float hungerThreshold = 50f;
    public float fatigueThreshold = 80f;

    [Header("Manger")]
    public int foodPerEat = 1; // unités prises par bouchée
    public float eatDurationPerUnit = 0.5f; // durée pour manger une unité
    public float eatRate = 20f; // points de faim / sec

    [Header("Mouvement")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 0.2f;

    [Header("Références")]
    public CityUtilityAI city;

    [Header("Construction fallback")]
    public float defaultBuildTime = 2f;

    private NavMeshAgent agent;
    private Animator animator;

    private enum EState { Idle, Moving, Working, Depositing, Eating, Sleeping }
    private EState state = EState.Idle;

    private CityTask currentTask;
    private Coroutine actionCoroutine;

    public int carrying = 0;
    public ResourceType carryingType = ResourceType.None;

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
        if (city == null) city = FindObjectOfType<CityUtilityAI>();
    }

    void Update()
    {
        hunger += hungerRate * Time.deltaTime;
        fatigue += fatigueRate * Time.deltaTime;

        if (hunger >= hungerThreshold && state != EState.Eating)
        {
            if (!isBusy) StartEatFlow();
        }

        if (fatigue >= fatigueThreshold && state != EState.Sleeping)
        {
            if (!isBusy) StartSleepFlow();
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
                actionCoroutine = StartCoroutine(CollectRoutine(task));
                break;
            case TaskType.Build:
                actionCoroutine = StartCoroutine(BuildRoutine(task));
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

    public bool IsIdle() => !isBusy && currentTask == null;
    #endregion

    #region EATING
    private void StartEatFlow()
    {
        StopCurrentAction();
        actionCoroutine = StartCoroutine(EatRoutine());
    }

    private IEnumerator EatRoutine()
    {
        StorageBuilding nearestStorage = FindNearestStorageWithFood();
        ResourceNode nearestFoodNode = FindNearestResource(ResourceType.Food);

        state = EState.Moving;

        if (nearestStorage != null)
        {
            if (!GoToPosition(nearestStorage.transform.position)) { StartIdle(); yield break; }
            yield return StartCoroutine(WaitUntilArrived());

            state = EState.Eating;

            while (hunger > 0f && nearestStorage.storedFood > 0)
            {
                int toTake = Mathf.Min(foodPerEat, nearestStorage.storedFood);
                int taken = nearestStorage.Withdraw(ResourceType.Food, toTake);
                if (taken <= 0) break;

                float elapsed = 0f;
                while (elapsed < eatDurationPerUnit && hunger > 0f)
                {
                    hunger -= eatRate * Time.deltaTime;
                    hunger = Mathf.Max(0f, hunger);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            StartIdle();
            yield break;
        }

        if (nearestFoodNode != null)
        {
            if (!GoToPosition(nearestFoodNode.transform.position)) { StartIdle(); yield break; }
            yield return StartCoroutine(WaitUntilArrived());

            state = EState.Eating;

            while (hunger > 0f && nearestFoodNode.amount > 0)
            {
                int toTake = Mathf.Min(foodPerEat, nearestFoodNode.amount);
                nearestFoodNode.amount -= toTake;

                float elapsed = 0f;
                while (elapsed < eatDurationPerUnit && hunger > 0f)
                {
                    hunger -= eatRate * Time.deltaTime;
                    hunger = Mathf.Max(0f, hunger);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            StartIdle();
            yield break;
        }

        StartIdle();
    }

    private StorageBuilding FindNearestStorageWithFood()
    {
        StorageBuilding best = null;
        float bestDist = float.PositiveInfinity;
        foreach (var s in FindObjectsOfType<StorageBuilding>())
        {
            if (s == null || s.storedFood <= 0) continue;
            float d = Vector3.Distance(transform.position, s.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = s;
            }
        }
        return best;
    }
    #endregion

    #region SLEEP
    private void StartSleepFlow()
    {
        StopCurrentAction();
        actionCoroutine = StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        GameObject house = FindNearestHouseObject();
        if (house == null) { StartIdle(); yield break; }

        state = EState.Moving;
        if (!GoToPosition(house.transform.position)) { StartIdle(); yield break; }
        yield return StartCoroutine(WaitUntilArrived());

        state = EState.Sleeping;
        float recoverRate = 25f;

        while (fatigue > 0f)
        {
            fatigue -= recoverRate * Time.deltaTime;
            fatigue = Mathf.Max(0f, fatigue);
            yield return null;
        }
        StartIdle();
    }

    private GameObject FindNearestHouseObject()
    {
        GameObject[] houses = GameObject.FindGameObjectsWithTag("House");
        if (houses.Length == 0) return null;

        GameObject best = null;
        float bestDist = float.PositiveInfinity;

        foreach (var h in houses)
        {
            float d = Vector3.Distance(transform.position, h.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = h;
            }
        }
        return best;
    }
    #endregion

    #region COLLECT / BUILD routines
    private IEnumerator CollectRoutine(CityTask task)
    {
        state = EState.Moving;
        if (task.resourceTarget == null) { FinishCurrentTask(); yield break; }
        if (!GoToPosition(task.resourceTarget.transform.position)) { FinishCurrentTask(); yield break; }
        yield return StartCoroutine(WaitUntilArrived());

        state = EState.Working;
        while (task.resourceTarget != null && task.resourceTarget.amount > 0)
        {
            int canTake = Mathf.Min(carryCapacity - carrying, harvestPerAction, task.resourceTarget.amount);
            if (canTake <= 0)
            {
                yield return StartCoroutine(HandleDepositFlow());
                break;
            }

            task.resourceTarget.amount -= canTake;
            carrying += canTake;
            carryingType = task.resourceTarget.resourceType;

            if (carrying >= carryCapacity)
                yield return StartCoroutine(HandleDepositFlow());

            yield return new WaitForSeconds(0.3f);
        }

        FinishCurrentTask();
    }

    private IEnumerator BuildRoutine(CityTask task)
    {
        state = EState.Moving;
        if (!GoToPosition(task.buildPosition)) { FinishCurrentTask(); yield break; }
        yield return StartCoroutine(WaitUntilArrived());

        state = EState.Working;
        float buildTime = (task.data != null && task.data.workDuration > 0f) ? task.data.workDuration : defaultBuildTime;
        float elapsed = 0f;
        while (elapsed < buildTime)
        {
            if (currentTask != task) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (task.buildingData != null && task.buildingData.prefab != null)
            Instantiate(task.buildingData.prefab, task.buildPosition, Quaternion.identity);

        FinishCurrentTask();
    }

    private IEnumerator HandleDepositFlow()
    {
        state = EState.Depositing;
        StorageBuilding storage = city?.FindNearestStorage(transform.position);

        if (storage == null)
        {
            city?.NotifyResourceCollected(carryingType, carrying);
            carrying = 0;
            carryingType = ResourceType.None;
            yield break;
        }

        if (!GoToPosition(storage.transform.position)) yield break;
        yield return StartCoroutine(WaitUntilArrived());

        storage.Deposit(carryingType, carrying);
        city?.NotifyResourceCollected(carryingType, carrying);

        carrying = 0;
        carryingType = ResourceType.None;

        yield return new WaitForSeconds(0.1f);
    }
    #endregion

    #region MOVEMENT HELPERS
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

    #region HELPERS
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

    private void FinishCurrentTask()
    {
        if (currentTask != null)
        {
            currentTask.isCompleted = true;
            currentTask.assignedVillagers.Remove(this);
        }
        currentTask = null;
        StartIdle();
    }

    private void StartIdle() => state = EState.Idle;

    private void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetBool("isMoving", state == EState.Moving);
        animator.SetBool("isWorking", state == EState.Working);
        animator.SetBool("isDepositing", state == EState.Depositing);
        animator.SetBool("isEating", state == EState.Eating);
        animator.SetBool("isSleeping", state == EState.Sleeping);
    }
    #endregion
}
