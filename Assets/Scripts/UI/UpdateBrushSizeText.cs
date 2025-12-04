using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateBrushSizeText : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI text;

    void OnEnable()
    {
        slider.onValueChanged.AddListener(ChangeValue);
    }

    private void ChangeValue(float pValue)
    {
        text.text = "Brush Size : " +  pValue;
    }
}