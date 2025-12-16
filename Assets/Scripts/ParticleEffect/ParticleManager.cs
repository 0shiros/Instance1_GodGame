using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;

    [SerializeField] private List<ParticleSystem> particleEffects = new List<ParticleSystem>();
    
    private ParticleSystem particleSystem;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    public void StartParticle(int pId)
    {
        if (pId > particleEffects.Count || pId < 0) return;
        
        if(particleEffects[pId] == null) return;
        
        particleEffects[pId].Play();
    }
}
