using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Quest : MonoBehaviour
{
    [SerializeField] List<SO_Quest> quests = new List<SO_Quest>();
    [SerializeField] private Image previewImage;
    [SerializeField] private TextMeshProUGUI questName;
    [SerializeField] private TextMeshProUGUI questDescription;
    public static Quest Instance;
    private SO_Quest activeSoQuest;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartQuest(0);
    }

    public void StartQuest(int pQuestID)
    {
        if (pQuestID > quests.Count - 1 || pQuestID < 0) return;

        if (quests[pQuestID].QuestImage != null)
        {
            previewImage.sprite = quests[pQuestID].QuestImage;
            previewImage.enabled = true;
        }
        else
        {
            previewImage.sprite = null;
            previewImage.enabled = false;
        }

        questName.text = quests[pQuestID].QuestName;
        questDescription.text = $"{quests[pQuestID].QuestDescription} \n 0 / {quests[pQuestID].QuestRequirement}";
        activeSoQuest = quests[pQuestID];
    }

    public void CompleteQuest(int pQuestID)
    {
        if (pQuestID > quests.Count - 1 || pQuestID < 0) return;

        if (activeSoQuest != null)
        {
            if (quests[pQuestID].QuestID == activeSoQuest.QuestID)
            {
                questDescription.text =
                    $"{quests[pQuestID].QuestDescription} \n 1 / {quests[pQuestID].QuestRequirement}";
                if (!quests[pQuestID].HasBeenCompleted)
                {
                    AudioManager.Instance.PlaySound("QuestComplete");
                    quests[pQuestID].HasBeenCompleted = true;
                }
                StartCoroutine(NextQuest(pQuestID));
            }
        }
    }

    private IEnumerator NextQuest(int pQuestID)
    {
        yield return new WaitForSeconds(2);

        pQuestID++;
        if (pQuestID > quests.Count - 1 || pQuestID < 0) yield break;

        StartQuest(pQuestID);
    }
}