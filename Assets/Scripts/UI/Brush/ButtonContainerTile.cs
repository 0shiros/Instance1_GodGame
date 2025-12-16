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
            if (Tiles.RuleTiles == null)
            {
                return;
            }
            
            image.sprite = Tiles.RuleTiles?.m_DefaultSprite;
            image.color = Tiles.Color;
            return;
        }

        if (CustomTile != null)
        {
            if (CustomTile.Sprite != null)
            {
                image.sprite = CustomTile.Sprite;
            }
        }
    }
}