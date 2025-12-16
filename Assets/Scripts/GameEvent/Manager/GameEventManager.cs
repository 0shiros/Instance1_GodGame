using UnityEngine;
using Random = UnityEngine.Random;

public class GameEventManager : MonoBehaviour
{
    public Vector2Int xLocationMinMax;
    public Vector2Int yLocationMinMax;
    public St_Event[] events;
    [HideInInspector] public bool IsRandom = true;

    // private void Update()
    // {
    //     if (Input.GetKeyDown("up")) TryInitializeEvent();
    // }

    void TryInitializeEvent()
    {
        int x = Random.Range(xLocationMinMax.x, xLocationMinMax.y);
        int y = Random.Range(yLocationMinMax.x, yLocationMinMax.y);
        foreach (St_Event e in events)
        {
            //condition de la popula° et villages ici
            float timer = Random.Range(0.0f , 60.0f);
            int random = Random.Range(0, 100);
            if (random > e.chanceActivation && IsRandom) continue;
            e.gameEvent.SetupEvent(x, y);
        }
    }
}

[System.Serializable]
public struct St_Event
{
    public string eventName;
    public int eventID;
    public GameEvent gameEvent;
    public int populationMin;
    public int villesMin;
    [Range(0, 100)]
    public int chanceActivation;
}


