using UnityEngine;

[CreateAssetMenu(fileName = "SO_AudioVolumes", menuName = "Scriptable Objects/SO_AudioVolumes")]
public class SO_AudioVolumes : ScriptableObject
{
    public float MasterVolume;
    public float MusicVolume;
    public float SFXVolume;
}
