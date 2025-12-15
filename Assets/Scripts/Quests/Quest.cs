using System;
using System.Collections.Generic;
using UnityEngine;

public class Quest : MonoBehaviour
{
    [SerializeField] List<Quests> quests = new List<Quests>();
    public static Quest Instance;
    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartQuest(0);
    }

    public void StartQuest(int questID) 
    {
        if(quests[questID].QuestProgress < 0) return;
        quests[questID].QuestPrompts[0].gameObject.SetActive(true);
    }

    public void Next(int questID, int progress) {
        if (quests[questID].QuestProgress + 1 != progress) return;
        quests[questID].QuestPrompts[progress - 1].gameObject.SetActive(false);
        if (progress >= quests[questID].QuestPrompts.Count) return;
        quests[questID].QuestPrompts[progress].gameObject.SetActive(true);
        quests[questID].QuestProgress++;
    }
}

[System.Serializable]
public class Quests 
{
    public string QuestName;
    public List<GameObject> QuestPrompts;
    [HideInInspector] public int QuestProgress;
    [HideInInspector] public bool QuestIsComplete;
}
