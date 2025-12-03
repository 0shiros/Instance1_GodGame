using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Technology 
{
    [SerializeField] TechnologyData technologyData;
    [SerializeField] E_Technologies technologyName;
    public int ExperienceNeedToUnlock;
    [SerializeField] List<E_Technologies> technologiesNeedToBeUnlock;


    public void Initialize(TechnologyData pData)
    {
        technologyData = pData;
        if (technologyData != null)
        {
            technologyName = technologyData.TechnologyName;
            ExperienceNeedToUnlock = technologyData.ExperienceNeedToUnlock;
            technologiesNeedToBeUnlock = technologyData.TechnologiesNeedToBeUnlock;
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
            if (pTechnologiesUnlock.All(tech => tech.technologyName != technology))
            {
                return false;
            }
            
        }
        
        return true;
    }
}
