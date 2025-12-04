using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Agent
{
    public int Hp;
    public int Speed;
    public int Strength; 
}
public class SetDogmaToPopulation : MonoBehaviour
{
    [SerializeField] private int agentsQuantity;
    [SerializeField] private int agentsQuantityNeedToSetDogma;
    [SerializeField] private List<Agent> agents;
    private float averageHp;
    private float averageSpeed;
    private float averageStrength;
    private static int hpMin = 10;
    private static int hpMax = 100;
    private static int speedMin = 1;
    private static int speedMax = 10;
    private static int strengthMin = 5;
    private static int strengthMax = 20;
    private static readonly float referenceHp = (hpMax + hpMin) / 2;
    private static readonly float referenceSpeed = (speedMax + speedMin) / 2;
    private static readonly float referenceStrength = (strengthMax + strengthMin) / 2;
    public E_Dogma CurrentDogma = E_Dogma.None;

    private void Start()
    {
        for (int i = 0; i < agentsQuantity; i++)
        {
            Agent newAgent = new Agent
            {
                Hp = UnityEngine.Random.Range(hpMin, hpMax),
                Speed = UnityEngine.Random.Range(speedMin, speedMax),
                Strength = UnityEngine.Random.Range(strengthMin, strengthMax)
            };
            
            agents.Add(newAgent);
        }
    }

    private void Update()
    {
        CalculateAverages();
        if (CurrentDogma == E_Dogma.None) SetDogma();
    }

    private void CalculateAverages()
    {
        float totalHp = 0;
        float totalSpeed = 0;
        float totalStrength = 0;

        foreach (var agent in agents)
        {
            totalHp += agent.Hp;
            totalSpeed += agent.Speed;
            totalStrength += agent.Strength;
        }

        averageHp = totalHp / agents.Count;
        averageSpeed = totalSpeed / agents.Count;
        averageStrength = totalStrength / agents.Count;
    }

    private void SetDogma()
    {
        float[] differences = {
            averageHp - referenceHp,
            averageSpeed - referenceSpeed,
            averageStrength - referenceStrength
        };
        
        Debug.Log($"Differences: HP={differences[0]},  Speed={differences[1]}, Strength={differences[2]}");
        
        if (agentsQuantity < agentsQuantityNeedToSetDogma)
            return;

        int maxIndex = 0;
        for (int i = 1; i < differences.Length; i++)
        {
            if (differences[i] > differences[maxIndex])
                maxIndex = i;
        }

        switch (maxIndex)
        {
            case 0: CurrentDogma = E_Dogma.Craft; break;
            case 1: CurrentDogma = E_Dogma.Development; break;
            case 2: CurrentDogma = E_Dogma.Military; break;
        }
        
    }
}
