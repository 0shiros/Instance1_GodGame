using System;
using TMPro;
using UnityEngine;

public class MapGenTweekerUI : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    [SerializeField] private SO_MapData mapData;

    private void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(SetMapData);
    }

    private void SetMapData(int pValue)
    {
        switch (pValue) 
        {
            case 0:
                mapData.MapBounds = new Vector2Int(100, 100);
                break;
            case 1:
                mapData.MapBounds = new Vector2Int(250, 250);
                break;
            case 2:
                mapData.MapBounds = new Vector2Int(500, 500);
                break;
            case 3:
                mapData.MapBounds = new Vector2Int(1000, 1000);
                break;
        }
    }
}
