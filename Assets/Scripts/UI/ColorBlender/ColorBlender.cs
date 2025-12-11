using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorBlender : MonoBehaviour
{
    [SerializeField] Image previewImage;
    [SerializeField] TMP_Dropdown spriteSelectorDropdown;

    [SerializeField] List<CustomTileData> saveTile = new List<CustomTileData>();
    public static ColorBlender Instance;

    private bool isUserSettingValues = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        spriteSelectorDropdown.onValueChanged.AddListener(DisplaySprite);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public Color BlendColorForTile()
    {
        return previewImage.color;
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

        previewImage.color = saveTile[pIndex].Color;
    }
    
    public void SaveColor()
    {
        if (saveTile.Count == 0) return;
        var data = saveTile[spriteSelectorDropdown.value];
        data.Color = previewImage.color;
        saveTile[spriteSelectorDropdown.value] = data;
    }
}