using UnityEngine;
using Random = UnityEngine.Random;

public class GameEventManager : MonoBehaviour
{
    [SerializeField] Vector2Int xLocationMinMax;
    [SerializeField] Vector2Int yLocationMinMax;
     public St_Event[] events;
    //ref à la clock du jeu

    private void Update()
    {
        if (Input.GetKeyDown("up")) TryInitializeEvent();
    }

    void TryInitializeEvent()
    {
        int x = Random.Range(xLocationMinMax.x, xLocationMinMax.y);
        int y = Random.Range(yLocationMinMax.x, yLocationMinMax.y);
        foreach (St_Event e in events)
        {
            //condition de la popula° et villages ici
            float timer = Random.Range(0.0f , 60.0f);
            int random = Random.Range(0, 100);
            if (random > e.chanceActivation) continue;
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


