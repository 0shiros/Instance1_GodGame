using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class VillagerUtilityAI : MonoBehaviour
{
    public VillagerRole role = VillagerRole.Generalist;

    [Header("Capacités")]
    public int CarryCapacity = 5;
    public int HarvestPerAction = 2;
    public float HarvestDistance = 2f;

    [Header("Besoins")]
    public float Hunger = 0f;
    public float Fatigue = 0f;
    public float HungerRate = 0.5f;
    public float FatigueRate = 0.2f;
    public float HungerThreshold = 50f;
    public float FatigueThreshold = 80f;

    [Header("Manger")]
    public int FoodPerEat = 1;
    public float EatDurationPerUnit = 0.5f;
    public float EatRate = 20f;

    [Header("Statistics")]
    public int Hp;
    public int Strength;

    [Header("Statistics limits")]
    public int HpMin = 85;
    public int HpMax = 100;
    public int SpeedMin = 5;
    public int SpeedMax = 20;
    public int StrengthMin = 5;
    public int StrengthMax = 20;

    [Header("Mouvement")]
    public float stoppingDistance = 0.2f;

    [Header("Références")]
    public CityUtilityAI city;

    [Header("Construction fallback")]
    public float defaultBuildTime = 2f;

    public NavMeshAgent agent;
    private Animator animator;

    private enum EState { Idle, Moving, Working, Depositing, Eating, Sleeping }
    private EState state = EState.Idle;

    private CityTask currentTask;
    private Coroutine actionCoroutine;

    public int carrying = 0;
    public ResourceType carryingType = ResourceType.None;

    public bool isBusy => state != EState.Idle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.stoppingDistance = stoppingDistance;
            Hp = Random.Range(HpMin, HpMax);
            agent.speed = Random.Range(SpeedMin, SpeedMax);
            Strength = Random.Range(StrengthMin, StrengthMax);
        }

        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (city == null) city = FindObjectOfType<CityUtilityAI>();
    }

    private void Update()
    {
        Hunger += HungerRate * Time.deltaTime;
        Fatigue += FatigueRate * Time.deltaTime;

        if (Hunger >= HungerThreshold && state != EState.Eating && !isBusy)
            StartEatFlow();

        if (Fatigue >= FatigueThreshold && state != EState.Sleeping && !isBusy)
            StartSleepFlow();

        UpdateAnimator();
    }

    #region Task Management
    public void AssignTask(CityTask task)
    {
        if (task == null || currentTask == task) return;

        StopCurrentAction();
        currentTask = task;

        switch (task.Data?.Type)
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
            currentTask.AassignedVillagers.Remove(this);
            currentTask = null;
        }
        StopCurrentAction();
        StartIdle();
    }

    public bool IsIdle() => !isBusy && currentTask == null;
    #endregion

    #region Eating
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
            yield return WaitUntilArrived();

            state = EState.Eating;

            while (Hunger > 0f && nearestStorage.StoredFood > 0)
            {
                int toTake = Mathf.Min(FoodPerEat, nearestStorage.StoredFood);
                int taken = nearestStorage.Withdraw(ResourceType.Food, toTake);
                if (taken <= 0) break;

                float elapsed = 0f;
                while (elapsed < EatDurationPerUnit && Hunger > 0f)
                {
                    Hunger -= EatRate * Time.deltaTime;
                    Hunger = Mathf.Max(0f, Hunger);
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
            yield return WaitUntilArrived();

            state = EState.Eating;

            while (Hunger > 0f && nearestFoodNode.Amount > 0)
            {
                if (Vector3.Distance(transform.position, nearestFoodNode.transform.position) > HarvestDistance) break;

                int toTake = Mathf.Min(FoodPerEat, nearestFoodNode.Amount);
                nearestFoodNode.Amount -= toTake;

                float elapsed = 0f;
                while (elapsed < EatDurationPerUnit && Hunger > 0f)
                {
                    Hunger -= EatRate * Time.deltaTime;
                    Hunger = Mathf.Max(0f, Hunger);
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
            if (s == null || s.StoredFood <= 0) continue;
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

    #region Sleep
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
        yield return WaitUntilArrived();

        state = EState.Sleeping;
        float recoverRate = 25f;

        while (Fatigue > 0f)
        {
            Fatigue -= recoverRate * Time.deltaTime;
            Fatigue = Mathf.Max(0f, Fatigue);
            yield return null;
        }
        StartIdle();
    }

    private GameObject FindNearestHouseObject()
    {
        GameObject[] houses = GameObject.FindGameObjectsWithTag("Building");
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

    #region Collect / Build routines
    private IEnumerator CollectRoutine(CityTask task)
    {
        if (task.ResourceTarget == null) { FinishCurrentTask(); yield break; }

        state = EState.Moving;
        if (!GoToPosition(task.ResourceTarget.transform.position)) { FinishCurrentTask(); yield break; }
        yield return WaitUntilArrived();

        state = EState.Working;

        while (task.ResourceTarget != null && task.ResourceTarget.Amount > 0)
        {
            if (Vector3.Distance(transform.position, task.ResourceTarget.transform.position) > HarvestDistance) break;

            int canTake = Mathf.Min(CarryCapacity - carrying, HarvestPerAction, task.ResourceTarget.Amount);
            if (canTake <= 0)
            {
                yield return HandleDepositFlow();
                break;
            }

            task.ResourceTarget.Amount -= canTake;
            carrying += canTake;
            carryingType = task.ResourceTarget.ResourceType;

            if (carrying >= CarryCapacity)
                yield return HandleDepositFlow();

            yield return new WaitForSeconds(0.3f);
        }
        FinishCurrentTask();
    }

    private IEnumerator BuildRoutine(CityTask task)
    {
        state = EState.Moving;
        if (!GoToPosition(task.BuildPosition)) { FinishCurrentTask(); yield break; }
        yield return WaitUntilArrived();

        state = EState.Working;
        float buildTime = (task.Data != null && task.Data.WorkDuration > 0f) ? task.Data.WorkDuration : defaultBuildTime;
        float elapsed = 0f;
        while (elapsed < buildTime)
        {
            if (currentTask != task) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (task.BuildingData?.Prefab != null)
            Instantiate(task.BuildingData.Prefab, task.BuildPosition, Quaternion.identity);

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
        yield return WaitUntilArrived();

        storage.Deposit(carryingType, carrying);
        city?.NotifyResourceCollected(carryingType, carrying);

        carrying = 0;
        carryingType = ResourceType.None;

        yield return new WaitForSeconds(0.1f);
    }
    #endregion

    #region Movement Helpers
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

    #region Helpers
    private ResourceNode FindNearestResource(ResourceType type)
    {
        ResourceNode best = null;
        float bestDist = float.PositiveInfinity;
        foreach (var rn in FindObjectsOfType<ResourceNode>())
        {
            if (rn == null || rn.ResourceType != type || rn.Amount <= 0) continue;
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
            currentTask.IsCompleted = true;
            currentTask.AassignedVillagers.Remove(this);
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
