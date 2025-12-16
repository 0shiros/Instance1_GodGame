using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonContainerTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Tiles != null)
        {
            ToolTip.Instance.ShowTooltip(Tiles.Name);
        }
        else if (CustomTile != null)
        {
            ToolTip.Instance.ShowTooltip(CustomTile.Name);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTip.Instance.HideTooltip();
    }
}