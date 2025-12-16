using UnityEngine;

[CreateAssetMenu(fileName = "SO_Quest", menuName = "Scriptable Objects/SO_Quest")]
public class SO_Quest : ScriptableObject
{
    public string QuestName;
    [Multiline] public string QuestDescription;
    public int QuestRequirement;
    public Sprite QuestImage;
    public int QuestID;
    
    [HideInInspector]
    public bool HasBeenCompleted = false;
}
