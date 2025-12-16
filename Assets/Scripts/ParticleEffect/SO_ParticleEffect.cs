using UnityEngine;

[CreateAssetMenu(fileName = "ParticleEffect", menuName = "Scriptable Objects/ParticleEffect")]
public class SO_ParticleEffect : ScriptableObject
{
    public int ID;
    public string Name;
    public ParticleSystem ParticleSystem;
}
