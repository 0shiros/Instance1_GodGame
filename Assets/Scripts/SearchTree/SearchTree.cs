using System;
using System.Collections.Generic;
using UnityEngine;

public class SearchTree : MonoBehaviour
{
   [SerializeField] private TechnologyData[] technologyData;
   [SerializeField] private int currentSciencePoints;
   [SerializeField] private List<Technology> technologiesAvailable;
   [SerializeField] private List<Technology> technologiesUnlock;

   private void Start()
   {
      SetTechnologiesAvailable();
   }

   private void Update()
   {
      UnlockTechnology();
   }

   private void SetTechnologiesAvailable()
   {
      foreach (TechnologyData technology in technologyData)
      {
         Technology newTechnology = new();
         newTechnology.Initialize(technology);
         technologiesAvailable.Add(newTechnology);
      }
      
      SortTechnologiesByExperienceRequired(technologiesAvailable);
   }

   private void SortTechnologiesByExperienceRequired(List<Technology> pTechnologies)
   { 
      pTechnologies.Sort((x, y) => x.ExperienceNeedToUnlock.CompareTo(y.ExperienceNeedToUnlock));
   }
   
   private void UnlockTechnology()
   {
      if(technologiesAvailable.Count <= 0) return;
      
      for(int i = 0; i < technologiesAvailable.Count; i++)
      {
         if (technologiesAvailable[i].CanUnlockTechnology(currentSciencePoints, technologiesUnlock))
         {
            technologiesUnlock.Add(technologiesAvailable[i]);
            technologiesAvailable.RemoveAt(i);
         }
      }
   }
}
