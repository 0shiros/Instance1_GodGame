using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonContainerTile : MonoBehaviour
{
    public SO_Tiles Tiles;
    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
        image.sprite = Tiles.RuleTiles?.m_DefaultSprite;
        image.color = Tiles.color;
    }
}
