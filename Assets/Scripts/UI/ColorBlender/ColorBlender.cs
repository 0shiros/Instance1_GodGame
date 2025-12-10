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

    [SerializeField] List<CustomTileData> saveTile = new List<CustomTileData>();
    public static ColorBlender Instance;

    private bool isUserSettingValues = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        redSlider.onValueChanged.AddListener(OnSliderChanged);
        greenSlider.onValueChanged.AddListener(OnSliderChanged);
        blueSlider.onValueChanged.AddListener(OnSliderChanged);
        alphaSlider.onValueChanged.AddListener(OnSliderChanged);

        spriteSelectorDropdown.onValueChanged.AddListener(DisplaySprite);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void OnSliderChanged(float value)
    {
        // Mise à jour instantanée de la couleur d’aperçu
        Color color = new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
        previewImage.color = color;

        // On sauvegarde UNIQUEMENT si les sliders ont été modifiés manuellement
        if (!isUserSettingValues) return;

        int idx = spriteSelectorDropdown.value;
        if (idx >= 0 && idx < saveTile.Count)
        {
            var customTileData = saveTile[idx];
            customTileData.Color = color;
            saveTile[idx] = customTileData;
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

        return BlendColorForTile();
    }

    public void SetColorForTile(SO_Tiles pTile)
    {
        spriteSelectorDropdown.options.Clear();
        saveTile.Clear();
        spriteSelectorDropdown.gameObject.SetActive(false);

        // Empêcher que changer les sliders déclenche une sauvegarde
        isUserSettingValues = false;

        redSlider.value = pTile.color.r;
        greenSlider.value = pTile.color.g;
        blueSlider.value = pTile.color.b;
        alphaSlider.value = pTile.color.a;

        isUserSettingValues = true;

        if (pTile.RuleTiles != null)
        {
            previewImage.sprite = pTile.RuleTiles.m_DefaultSprite;
            previewImage.color = pTile.color;
        }
        else
        {
            previewImage.sprite = null;
            previewImage.color = Color.clear;
        }
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

        // Bloquer la sauvegarde automatique
        isUserSettingValues = false;

        redSlider.value = saveTile[pIndex].Color.r;
        greenSlider.value = saveTile[pIndex].Color.g;
        blueSlider.value = saveTile[pIndex].Color.b;
        alphaSlider.value = saveTile[pIndex].Color.a;

        previewImage.color = saveTile[pIndex].Color;

        // Réautoriser la sauvegarde après mise à jour
        isUserSettingValues = true;
    }
}
