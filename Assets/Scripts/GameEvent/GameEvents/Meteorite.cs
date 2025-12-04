using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Meteorite : GameEvent
{
    [SerializeField] int ID;
    [SerializeField] Vector2Int radiusMinMax;
    [SerializeField] Vector2Int activationMinMax;
    [SerializeField] Vector2 delayMinMax;
    [SerializeField] GameEventBrush brush;
    [SerializeField] SO_Tiles meteoriteTile;
    float timer;

    public override void SetupEvent(int x, int y, float pTimer = 0)
    {
        Debug.Log("Setup Meteorite");
        int radius = Random.Range(radiusMinMax.x, radiusMinMax.y);
        int activation = Random.Range(activationMinMax.x, activationMinMax.y);
        StartCoroutine(StartMeteorite(radius, activation, new Vector3Int(x, y), pTimer));
    }
    
    IEnumerator StartMeteorite(int pRadius, int pActivationTimer, Vector3Int pLocation, float pTimer)
    {
        while (timer < pTimer)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        while (timer < pActivationTimer)
        {
            //jouer Anim ici
            timer += Time.deltaTime;
            yield return null;
        }
        float delay = Random.Range(delayMinMax.x, delayMinMax.y);
        StartCoroutine(brush.CircleDraw(meteoriteTile, pLocation, pRadius, delay));
    }
}
