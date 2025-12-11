// Enums.cs
using UnityEngine;

public enum ResourceType
{
    None,
    Wood,
    Stone,
    Food,
    Maital
}

public enum TaskType
{
    Collect,
    Build,
    Deposit,
    Idle,
    exploring,
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
