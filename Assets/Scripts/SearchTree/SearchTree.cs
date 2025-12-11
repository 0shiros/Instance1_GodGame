
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SearchTree : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TechnologyData[] technologiesData;
    private CityUtilityAI cityUtilityAI;
    private NationIdentity nationIdentity;

    [Header("TechSettings")]
    [SerializeField] private int currentSciencePoints;
    [SerializeField] private int currentDogmaSciencePoints;
    [SerializeField] private List<Technology> TechnologiesAvailable;
    [SerializeField] private List<Technology> TechnologiesUnlock;

    [Header("UIStatistics")]
    private int basicTechUnlockQuantity;
    private int basicTechUnlockQuantityMax;
    private int dogmaTechUnlockQuantity;
    private int dogmaTechUnlockQuantityMax;

    private void OnEnable()
    {
        CityUtilityAI.ActionBasic += AddExperience;
        CityUtilityAI.ActionDogma += AddDogmaExperience;
    }
    private void OnDisable()
    {
        CityUtilityAI.ActionBasic -= AddExperience;
        CityUtilityAI.ActionDogma -= AddDogmaExperience;
    }

    private void AddExperience(int pExperienceReward)
    {
        currentSciencePoints += pExperienceReward;
    }

    private void AddDogmaExperience(int pExperienceReward)
    {
        currentDogmaSciencePoints += pExperienceReward;
    }

private void Start()
{
    // nationIdentity = gameObject.GetComponent<NationIdentity>();
    cityUtilityAI = gameObject.GetComponent<CityUtilityAI>();
    SetMaxQuantityOfBasicTech();
    SetMaxQuantityOfDogmaTech();
    //nationIdentity.SetTitle();
    //nationIdentity.SetBasicSearchTreeProgressBar(basicTechUnlockQuantity, basicTechUnlockQuantityMax);
    //nationIdentity.SetDogmaSearchTreeProgressBar(dogmaTechUnlockQuantity, dogmaTechUnlockQuantityMax);
}

private void Update()
{
    SetTechnologiesAvailable();
    UnlockTechnology();
}

private void SetMaxQuantityOfBasicTech()
{
    foreach (TechnologyData technologyData in technologiesData)
    {
        if (technologyData.Dogma == E_Dogma.None)
        {
            Debug.Log(technologyData);
            basicTechUnlockQuantityMax++;
        }
    }
}

private void SetMaxQuantityOfDogmaTech()
{
    foreach (TechnologyData technologyData in technologiesData)
    {
        if (technologyData.Dogma == cityUtilityAI.CurrentDogma)
        {
            Debug.Log(technologyData);
            dogmaTechUnlockQuantityMax++;
        }
    }
}

private void SetTechnologiesAvailable()
{
    foreach (TechnologyData technologyData in technologiesData)
    {
        E_Dogma dogma = technologyData.Dogma;
        Debug.Log(dogma);

        if (dogma == E_Dogma.None || dogma == cityUtilityAI.CurrentDogma)
        {
            Technology newTechnology = new();
            newTechnology.Initialize(technologyData);

            if (TechnologiesAvailable.All(tech => tech.TechnologyName != newTechnology.TechnologyName) &&
                TechnologiesUnlock.All(tech => tech.TechnologyName != newTechnology.TechnologyName))
            {
                TechnologiesAvailable.Add(newTechnology);
            }
        }
    }

    SortTechnologiesByExperienceRequired(TechnologiesAvailable);
}

private void SortTechnologiesByExperienceRequired(List<Technology> pTechnologies)
{
    pTechnologies.Sort((x, y) => x.ExperienceNeedToUnlock.CompareTo(y.ExperienceNeedToUnlock));
}

private void UnlockTechnology()
{
    if (TechnologiesAvailable.Count <= 0) return;

    List<Technology> toUnlock = TechnologiesAvailable.Where(tech =>
    {
        if (tech.Dogma == E_Dogma.None)
            return tech.CanUnlockTechnology(currentSciencePoints, TechnologiesUnlock);
        else
            return tech.CanUnlockTechnology(currentDogmaSciencePoints, TechnologiesUnlock);
    }).ToList();

    foreach (Technology tech in toUnlock)
    {
        if (tech.Dogma == E_Dogma.None)
        {
            basicTechUnlockQuantity++;
            //nationIdentity.SetBasicSearchTreeProgressBar(basicTechUnlockQuantity, basicTechUnlockQuantityMax);
        }
        else
        {
            dogmaTechUnlockQuantity++;
            //nationIdentity.SetDogmaSearchTreeProgressBar(dogmaTechUnlockQuantity, dogmaTechUnlockQuantityMax);
        }
        Debug.Log(tech.technologyData.Buildings);
            for (int i = 0; i < tech.Buildings.Count; i++)
            {
                cityUtilityAI.BuildingTypes.Add(tech.Buildings[i]);
            }
        TechnologiesUnlock.Add(tech);
        Debug.Log(tech.TechnologyName);
        TechnologiesAvailable.Remove(tech);
    }
}
}
