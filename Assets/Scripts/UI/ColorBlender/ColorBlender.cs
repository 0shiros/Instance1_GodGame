using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorBlender : MonoBehaviour
{
    [SerializeField] Slider redSlider;
    [SerializeField] Slider greenSlider;
    [SerializeField] Slider blueSlider;
    [SerializeField] Slider alphaSlider;
    [SerializeField] Image previewImage;
    [SerializeField] TMP_Dropdown spriteSelectorDropdown;

    private List<CustomTileData> saveTile = new List<CustomTileData>();
    public static ColorBlender Instance;

    void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        redSlider.onValueChanged.AddListener(ModifieColor);
        greenSlider.onValueChanged.AddListener(ModifieColor);
        blueSlider.onValueChanged.AddListener(ModifieColor);
        alphaSlider.onValueChanged.AddListener(ModifieColor);

        spriteSelectorDropdown.onValueChanged.AddListener(DisplaySprite);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void ModifieColor(float pValue)
    {
        Color color = new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
        previewImage.color = color;
        if (saveTile.Count > 0)
        {
            int idx = spriteSelectorDropdown.value;
            if (idx >= 0 && idx < saveTile.Count)
            {
                var data = saveTile[idx];
                data.Color = color;
                saveTile[idx] = data;
            }
        }
    }

    public Color BlendColorForTile()
    {
        return new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
    }

    public Color BlendColorForCustomTile(int pIndex)
    {
        if (pIndex >= 0 && pIndex < saveTile.Count)
            return saveTile[pIndex].Color;

        return new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
    }

    public void SetColorForTile(SO_Tiles pTile)
    {
        spriteSelectorDropdown.options.Clear();
        saveTile.Clear();
        spriteSelectorDropdown.gameObject.SetActive(false);
        redSlider.value = pTile.color.r;
        greenSlider.value = pTile.color.g;
        blueSlider.value = pTile.color.b;
        alphaSlider.value = pTile.color.a;
        previewImage.sprite = pTile.RuleTiles.m_DefaultSprite;
        // appliquer la couleur immédiatement
        previewImage.color = pTile.color;
    }

    public void SetColorForCustomTile(CustomTile pCustomTile)
    {
        if (pCustomTile.Sources.Count != 0)
            spriteSelectorDropdown.gameObject.SetActive(true);

        spriteSelectorDropdown.options.Clear();
        saveTile.Clear();
        foreach (CustomTileData data in pCustomTile.Sources)
        {
            spriteSelectorDropdown.options.Add(new TMP_Dropdown.OptionData(data.Name, data.Sprites.sprite, data.Color));
            saveTile.Add(data);
        }

        if (spriteSelectorDropdown.options.Count > 0)
        {
            spriteSelectorDropdown.value = 0;
            spriteSelectorDropdown.RefreshShownValue();
            DisplaySprite(0);
        }
    }

    private void DisplaySprite(int pIndex)
    {
        if (pIndex < 0 || pIndex >= spriteSelectorDropdown.options.Count || pIndex >= saveTile.Count) return;

        previewImage.sprite = spriteSelectorDropdown.options[pIndex].image;
        redSlider.value = saveTile[pIndex].Color.r;
        greenSlider.value = saveTile[pIndex].Color.g;
        blueSlider.value = saveTile[pIndex].Color.b;
        alphaSlider.value = saveTile[pIndex].Color.a;
    }
}
