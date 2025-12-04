using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SearchTree : MonoBehaviour
{
   [SerializeField] private TechnologyData[] technologiesData;
   [SerializeField] private int currentSciencePoints;
   [SerializeField] private List<Technology> technologiesAvailable;
   [SerializeField] private List<Technology> technologiesUnlock;
   private SetDogmaToPopulation setDogmaToPopulation;
   [SerializeField] private int currentDogmaSciencePoints;

   private void Start()
   {
      setDogmaToPopulation = gameObject.GetComponent<SetDogmaToPopulation>();
   }

   private void Update()
   {
      SetTechnologiesAvailable();
      UnlockTechnology();
   }

   private void SetTechnologiesAvailable()
   {
      foreach (TechnologyData technologyData in technologiesData)
      {
         E_Dogma dogma = technologyData.Dogma;
         Debug.Log(dogma);
         
         if (dogma == E_Dogma.None || dogma == setDogmaToPopulation.CurrentDogma)
         {
            Technology newTechnology = new();
            newTechnology.Initialize(technologyData);
            
            if (technologiesAvailable.All(tech => tech.TechnologyName != newTechnology.TechnologyName) && 
                technologiesUnlock.All(tech => tech.TechnologyName != newTechnology.TechnologyName))
            {
               technologiesAvailable.Add(newTechnology);
            }
         }
      }
      
      SortTechnologiesByExperienceRequired(technologiesAvailable);
   }

   private void SortTechnologiesByExperienceRequired(List<Technology> pTechnologies)
   { 
      pTechnologies.Sort((x, y) => x.ExperienceNeedToUnlock.CompareTo(y.ExperienceNeedToUnlock));
   }
   
   private void UnlockTechnology()
   {
      if (technologiesAvailable.Count <= 0) return;

      List<Technology> toUnlock = technologiesAvailable.Where(tech =>
      {
         if (tech.Dogma == E_Dogma.None)
            return tech.CanUnlockTechnology(currentSciencePoints, technologiesUnlock);
         else
            return tech.CanUnlockTechnology(currentDogmaSciencePoints, technologiesUnlock);
      }).ToList();

      foreach (Technology tech in toUnlock)
      {
         technologiesUnlock.Add(tech);
         Debug.Log(tech.TechnologyName);
         technologiesAvailable.Remove(tech);
      }
   }
}
