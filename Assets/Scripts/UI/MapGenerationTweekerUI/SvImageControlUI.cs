using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SvImageControlUI : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    [SerializeField] private Image pickerImage;

    private RawImage svImage;

    [SerializeField] private ColorPickerUI colorPicker;

    private RectTransform rectTransform, pickerRectTransform;
    [SerializeField]
    private Camera cam;

    private void Awake()
    {
        svImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();

        pickerRectTransform = pickerImage.GetComponent<RectTransform>();
        pickerRectTransform.position =
            new Vector2(-(rectTransform.sizeDelta.x * 0.5f), -(rectTransform.sizeDelta.y * 0.5f));
        colorPicker.OnSetPickerPos.AddListener(SetPickerPos);
    }

    private void SetPickerPos()
    {
        var halfX = rectTransform.sizeDelta.x * 0.5f;
        var halfY = rectTransform.sizeDelta.y * 0.5f;

        float x = Mathf.Lerp(-halfX, halfX, colorPicker.CurrentSet);
        float y = Mathf.Lerp(-halfY, halfY, colorPicker.CurrentVal);

        Vector2 localPos = new(x, y);
        pickerRectTransform.localPosition = localPos;
    }

    private void UpdateColor(PointerEventData pEventData)
    {
        Vector3 pos = rectTransform.InverseTransformPoint(pEventData.position);

        float deltaX = rectTransform.sizeDelta.x * 0.5f;
        float deltaY = rectTransform.sizeDelta.y * 0.5f;

        pos.x = Mathf.Clamp(pos.x, -deltaX, deltaX);
        pos.y = Mathf.Clamp(pos.y, -deltaY, deltaY);

        float x = pos.x + deltaX;
        float y = pos.y + deltaY;

        float xNorm = x / rectTransform.sizeDelta.x;
        float yNorm = y / rectTransform.sizeDelta.y;

        pickerRectTransform.localPosition = pos;
        pickerImage.color = Color.HSVToRGB(0, 0, 1 - yNorm);

        colorPicker.SetSV(xNorm, yNorm);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }
}