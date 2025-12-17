// Enums.cs
using UnityEngine;

public enum ResourceType
{
    None,
    Wood,
    Stone,
    Food,
    Metal
}

public enum TaskType
{
    Collect,
    Build,
    Deposit,
    Idle,
    Exploration,
    Combat,
    Custom
}

public enum BuildingType
{
    None,
    House,
    Warehouse,
    Farm,
    Forge,
    Granary,
    HouseStone,
    WarehouseStone,
    FarmStone,
    ForgeStone,
    GranaryStone,
    HouseMetal,
    WarehouseMetal,
    FarmMetal,
    ForgeMetal,
    GranaryMetal,
    Mine,
    Custom

}

public enum VillagerRole
{
    Generalist,
    Builder,
    Gatherer,
    Hauler
}
