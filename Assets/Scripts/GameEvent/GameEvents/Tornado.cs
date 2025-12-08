using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Tornado : GameEvent
{
    [SerializeField] int ID;
    [SerializeField] Vector2Int radiusMinMax;
    [SerializeField] Vector2 activationMinMax;
    [SerializeField] Vector2 durationMinMax;
    [SerializeField] GameEventBrush brush;
    [SerializeField] SO_Tiles SO_Tile;
    [SerializeField] List<Tilemap> tilemaps;
    [SerializedDictionary("id", "List")]
    Dictionary<int, ListWrapper> targets = new Dictionary<int, ListWrapper>();
    float animTimer;
    private int temp2;
    
    #region Movement
    
    [SerializeField] Vector2 speedMinMax;
    [SerializeField] GameObject tornado;
    bool canMove;

    #endregion

    public override void SetupEvent(int x, int y, float pTimer = 0)
    {
        Debug.Log("Setup Meteorite");
        int radius = Random.Range(radiusMinMax.x, radiusMinMax.y);
        float activation = Random.Range(activationMinMax.x, activationMinMax.y);
        float duration = Random.Range(durationMinMax.x, durationMinMax.y);
        float speed = Random.Range(speedMinMax.x, speedMinMax.y);

        int temp = Random.Range(1, targets.Count);
        transform.position = Vector3.MoveTowards(transform.position,
            targets[temp].target[Random.Range(0, targets[temp].target.Count - 1)].transform.position,
            speed * Time.deltaTime);
        
        temp2 = Random.Range(1, targets.Count);
        while (temp == temp2)
        {
            temp2 = Random.Range(1, targets.Count);
        }
        
        StartCoroutine(StartTornado(radius, activation, pTimer, duration, speed));
    }

    IEnumerator StartTornado(int pRadius, float pActivationTimer, float pTimer, float pDuration, float speed)
    {
        while (animTimer < pTimer)
        {
            animTimer += Time.deltaTime;
            yield return null;
        }
        animTimer = 0;
        while (animTimer < pActivationTimer)
        {
            animTimer += Time.deltaTime;
            yield return null;
        }
        animTimer = 0;
        while (animTimer < pDuration)
        {
            //loop de l'anim et déplacments
            transform.position = Vector3.MoveTowards(transform.position,
                targets[temp2].target[Random.Range(0, targets[temp2].target.Count - 1)].transform.position,
                speed * Time.deltaTime);
            Vector3Int midcell = tilemaps[0].WorldToCell(transform.position);
            StartCoroutine(brush.CircleDraw(SO_Tile, midcell, pRadius, tilemaps));
            yield return null;
        }
    }
    
    [System.Serializable] public class ListWrapper { public List<GameObject> target; }
}
