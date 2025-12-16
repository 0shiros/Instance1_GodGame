using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Meteorite : GameEvent
{
    [SerializeField] int ID;
    [SerializeField] Vector2Int radiusMinMax;
    [SerializeField] Vector2Int activationMinMax;
    [SerializeField] Vector2 delayMinMax;
    [SerializeField] GameEventBrush brush;
    [SerializeField] List<Tilemap> tilemaps;
    [SerializeField] SO_Tiles SO_Tile;
    [SerializeField] GameObject meteoriteEntity;
    float timer;

    private void Awake()
    {
        meteoriteEntity.SetActive(false);
    }

    public override void SetupEvent(int x, int y, float pTimer = 0)
    {
        Debug.Log("Setup Meteorite");
        int radius = Random.Range(radiusMinMax.x, radiusMinMax.y);
        int activation = Random.Range(activationMinMax.x, activationMinMax.y);
        meteoriteEntity.SetActive(true);
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
        meteoriteEntity.transform.position = pLocation+new Vector3Int(10, 10);
        meteoriteEntity.SetActive(true);
        meteoriteEntity.transform.DOMove(pLocation, pActivationTimer).SetEase(Ease.InQuint);
        while (timer < pActivationTimer)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        float delay = Random.Range(delayMinMax.x, delayMinMax.y);
        meteoriteEntity.SetActive(false);
        StartCoroutine(brush.CircleDraw(SO_Tile, pLocation, pRadius, tilemaps, delay));
    }
}
