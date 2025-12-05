using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonContainerTile : MonoBehaviour
{
    public SO_Tiles Tiles;
    public CustomTile CustomTile;
    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
        if (Tiles != null)
        {
            image.sprite = Tiles.RuleTiles?.m_DefaultSprite;
            image.color = Tiles.color;
            return;
        }
        
        if (CustomTile != null)
        {
            image.sprite = CustomTile.Sources[0].Sprites.sprite;
            image.color = CustomTile.Sources[0].Color;
        }
    }
}