using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Technology 
{
    [SerializeField] TechnologyData technologyData;
    public E_Technologies TechnologyName;
    public int ExperienceNeedToUnlock;
    [SerializeField] List<E_Technologies> technologiesNeedToBeUnlock;
    public E_Dogma Dogma;


    public void Initialize(TechnologyData pData)
    {
        technologyData = pData;
        if (technologyData != null)
        {
            TechnologyName = technologyData.TechnologyName;
            ExperienceNeedToUnlock = technologyData.ExperienceNeedToUnlock;
            technologiesNeedToBeUnlock = technologyData.TechnologiesNeedToBeUnlock;
            Dogma = technologyData.Dogma;
        }
    }

    public bool CanUnlockTechnology(int pSciencePointsNation, List<Technology> pTechnologiesUnlock)
    {
        if (pSciencePointsNation < ExperienceNeedToUnlock)
        {
            return false;
        }
        
        if(technologiesNeedToBeUnlock.Count <= 0) return true;

        foreach (E_Technologies technology in technologiesNeedToBeUnlock)
        {
            if (pTechnologiesUnlock.All(tech => tech.TechnologyName != technology))
            {
                return false;
            }
            
        }
        
        return true;
    }
}
