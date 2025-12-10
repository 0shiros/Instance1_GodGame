using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    public float CurrentHue, CurrentSet, CurrentVal;

    [SerializeField] private RawImage hueImage, satValImage, outputImage;

    [SerializeField] private Slider hueSlider;
    [SerializeField] private ColorBlender colorBlender;

    [SerializeField] private TMP_InputField hexInputField;

    private Texture2D hueTe, svTexture, outputTex;

    [SerializeField] private Image testImage;

    private void Start()
    {
        CreateHueImage();
        CreateSVImage();
        CreateOutputImage();

        UpdateOutputImage();
    }

    private void CreateHueImage()
    {
        hueTe = new Texture2D(1, 16);
        hueTe.wrapMode = TextureWrapMode.Clamp;
        hueTe.name = "HueTexture";

        for (int i = 0; i < hueTe.height; i++)
        {
            hueTe.SetPixel(0, i, Color.HSVToRGB((float)i / hueTe.height, 1, 1f));
        }

        hueTe.Apply();
        CurrentHue = 0;

        hueImage.texture = hueTe;
    }

    private void CreateSVImage()
    {
        svTexture = new Texture2D(16, 16);
        svTexture.wrapMode = TextureWrapMode.Clamp;
        svTexture.name = "SatValTexture";

        for (int x = 0; x < svTexture.width; x++)
        {
            for (int y = 0; y < svTexture.height; y++)
            {
                svTexture.SetPixel(x, y,
                    Color.HSVToRGB(CurrentHue, (float)x / svTexture.width, (float)y / svTexture.height));
            }
        }

        svTexture.Apply();
        CurrentSet = 0;
        CurrentVal = 0;

        satValImage.texture = svTexture;
    }

    private void CreateOutputImage()
    {
        outputTex = new Texture2D(1, 16);
        outputTex.wrapMode = TextureWrapMode.Clamp;
        outputTex.name = "OutputTexture";

        Color currentColor = Color.HSVToRGB(CurrentHue, CurrentSet, CurrentVal);

        for (int i = 0; i < outputTex.height; i++)
        {
            outputTex.SetPixel(0, i, currentColor);
        }

        outputTex.Apply();

        outputImage.texture = outputTex;
    }

    private void UpdateOutputImage()
    {
        Color currentColor = Color.HSVToRGB(CurrentHue, CurrentSet, CurrentVal);
        for (int i = 0; i < outputTex.height; i++)
        {
            outputTex.SetPixel(0, i, currentColor);
        }

        outputTex.Apply();
        
        hexInputField.text = ColorUtility.ToHtmlStringRGB(currentColor);     
        
        testImage.color = currentColor;
        colorBlender.SaveColor();
    }

    public void SetSV(float pS, float pV)
    {
        CurrentSet = pS;
        Debug.Log(CurrentSet);
        CurrentVal = pV;
        Debug.Log(CurrentVal);
        UpdateOutputImage();
    }

    public void UpdateSVImage()
    {
        CurrentHue = hueSlider.value;

        for (int y = 0; y < svTexture.height; y++)
        {
            for (int x = 0; x < svTexture.width; x++)
            {
                svTexture.SetPixel(x, y,
                    Color.HSVToRGB(CurrentHue, (float)x / svTexture.width, (float)y / svTexture.height));
            }
        }

        svTexture.Apply();

        UpdateOutputImage();
    }

    public void OnTextInput()
    {
        if (hexInputField.text.Length < 6)
        {
            return;
        }

        Color newCol;

        if (ColorUtility.TryParseHtmlString("#" + hexInputField.text, out newCol))
            Color.RGBToHSV(newCol, out CurrentHue, out CurrentSet, out CurrentVal);
        
        hueSlider.value = CurrentHue;
        hexInputField.text = "";
        UpdateOutputImage();
    }
}