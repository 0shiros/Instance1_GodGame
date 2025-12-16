using UnityEngine;

[CreateAssetMenu(fileName = "NewTaskData", menuName = "CityAI/Task Data")]
public class TaskData : ScriptableObject
{
    [Header("Informations générales")]
    [Tooltip("Nom lisible utilisé dans l'inspecteur et par l'UI si nécessaire.")]
    public string TaskName = "Nouvelle Tâche";

    [Tooltip("Type principal de la tâche (Collect, Build, etc.)")]
    public TaskType Type = TaskType.Collect;

    [Header("Paramètres de Collecte")]
    [Tooltip("Type de ressource que cette tâche vise à récolter.")]
    public ResourceType TargetResource = ResourceType.None;

    [Header("Paramètres Construction")]
    [Tooltip("Type de bâtiment que cette tâche doit construire.")]
    public BuildingType TargetBuildingType = BuildingType.None;

    [Header("Priorité / Utility AI")]
    [Tooltip("Priorité de base. Plus elle est élevée, plus la tâche sera attractive.")]
    [Range(0f, 10f)]
    public float BasePriority = 1f;

    [Tooltip("Nombre conseillé de villageois simultanés sur cette tâche (valeur indicatrice).")]
    [Min(1)]
    public int RecommendedVillagers = 1;

    [Header("Durée & travail")]
    [Tooltip("Durée estimée du travail pour cette tâche (secondes). Utilisé par les villageois pour simuler la construction).")]
    [Min(0f)]
    public float WorkDuration = 2f;
}
