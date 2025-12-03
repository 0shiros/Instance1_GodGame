// Enums.cs
using UnityEngine;

public enum ResourceType
{
    None,
    Wood,
    Stone,
    Food
}

public enum TaskType
{
    Collect,
    Build,
    Deposit,
    Idle,
    Custom
}

public enum BuildingType
{
    None,
    House,
    Warehouse,
    Farm,
    Custom
}

public enum VillagerRole
{
    Generalist,
    Builder,
    Gatherer,
    Hauler
}
