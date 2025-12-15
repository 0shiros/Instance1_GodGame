using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/TechnologyData", order = 1)]
public class TechnologyData : ScriptableObject
{
    public E_Technologies TechnologyName;
    public int ExperienceNeedToUnlock;
    public List<E_Technologies> TechnologiesNeedToBeUnlock;
    public E_Dogma Dogma;
    public List<BuildingData> Buildings;
    public St_Bonus Bonuses;
}

[System.Serializable]
public struct St_Bonus
{
    public int HpBonus;
    public int SpeedBonus;
    public int StrenghtBonus;
}