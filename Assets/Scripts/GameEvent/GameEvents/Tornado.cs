using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Tornado : GameEvent
{
    [SerializeField] int ID;
    [SerializeField] Vector2Int radiusMinMax;
    [SerializeField] GameEventBrush brush;
    [SerializeField] SO_Tiles SO_Tile;
    [SerializeField] List<Tilemap> tilemaps;
    [SerializeField] CustomDict targets;
    
    float animTimer;
    private int temp2;
    
    #region Movement
    
    [SerializeField] Vector2 speedMinMax;
    [SerializeField] GameObject tornado;
    bool canMove;

    #endregion

    public override void SetupEvent(int x, int y, float pTimer = 0)
    {
        Debug.Log("Setup Tornado");
        int radius = Random.Range(radiusMinMax.x, radiusMinMax.y);
        float speed = Random.Range(speedMinMax.x, speedMinMax.y);

        int temp = Random.Range(0, targets.sides.Count);

        temp2 = Random.Range(0, targets.sides.Count);
        while (temp == temp2)
        {
            temp2 = Random.Range(0, targets.sides.Count);
        }

        tornado.transform.position = targets.sides[temp].target[Random.Range(0, targets.sides[temp].target.Count)].transform.position;
        Vector3 targetPos = targets.sides[temp2].target[Random.Range(0, targets.sides[temp2].target.Count)].transform.position;
        StartCoroutine(StartTornado(radius, pTimer, speed, targetPos));
    }

    IEnumerator StartTornado(int pRadius, float pTimer, float pSpeed, Vector3 pTargetPos)
    {
        animTimer = 0;
        while (animTimer < pTimer)
        {
            animTimer += Time.deltaTime;
            yield return null;
        }
        animTimer = 0;
        var dist = Vector3.Distance(tornado.transform.position, pTargetPos);
        while (dist > 1)
        {
            dist = Vector3.Distance(tornado.transform.position, pTargetPos);
            //loop de l'anim et déplacments
            tornado.transform.position = Vector3.MoveTowards(tornado.transform.position, pTargetPos, pSpeed * Time.deltaTime);
            Vector3Int midcell = tilemaps[0].WorldToCell(transform.position);
            StartCoroutine(brush.CircleDraw(SO_Tile, midcell, pRadius, tilemaps));
            yield return null;
        }
    }
    
    [System.Serializable] public class CustomDict { public List<ListWrapper> sides; }
    
    [System.Serializable] public class ListWrapper { public List<GameObject> target; }
}
