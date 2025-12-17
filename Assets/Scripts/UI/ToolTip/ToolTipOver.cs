using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipOver : MonoBehaviour
{

    private void OnMouseEnter()
    {
        ToolTip.Instance.ShowTooltip("Press A for info");
    }

    private void OnMouseExit()
    {
        ToolTip.Instance.HideTooltip();
    }
}
