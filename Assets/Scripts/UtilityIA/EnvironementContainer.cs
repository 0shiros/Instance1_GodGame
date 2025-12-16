using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class EnvironementContainer : MonoBehaviour
{
    public static EnvironementContainer Instance;

    public List<ResourceNode> resourceNodes = new List<ResourceNode>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {

            Instance = this;
        ResourceNode.ActionResource += AddGOResource;

    }
   
    public void AddGOResource(ResourceNode pRessou)
    {
        resourceNodes.Add(pRessou);
        
    }

}
