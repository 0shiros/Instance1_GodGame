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
    [SerializeField] Vector2Int meteoriteStartPos;
    float timer;
    Camera camera;

    private void Awake()
    {
        meteoriteEntity.SetActive(false);
        camera = Camera.main;
    }

    public override void SetupEvent(int x, int y, float pTimer = 0)
    {
        Debug.Log("Setup Meteorite");
        int radius = Random.Range(radiusMinMax.x, radiusMinMax.y);
        int activation = Random.Range(activationMinMax.x, activationMinMax.y);
        meteoriteEntity.SetActive(true);
        Vector3Int tempVector3Int = tilemaps[0].WorldToCell(camera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2)));
        StartCoroutine(StartMeteorite(radius, activation, tempVector3Int, pTimer));
    }
    
    IEnumerator StartMeteorite(int pRadius, int pActivationTimer, Vector3Int pLocation, float pTimer)
    {
        while (timer < pTimer)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        meteoriteEntity.transform.position = pLocation + new Vector3Int(meteoriteStartPos.x, meteoriteStartPos.y);
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
