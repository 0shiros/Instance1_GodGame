using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NationIdentityRefs : MonoBehaviour
{
    [Header("References")]
    public GameObject Title;
    public GameObject Population;
    public GameObject Dogma;
    public GameObject Food;
    public GameObject Wood;
    public GameObject Stone;
    public GameObject Metal;
    public GameObject BasicSearchTreeProgressBar;
    public GameObject DogmaSearchTreeProgressBar;
    public GameObject CurrentBasicSearchTreeProgressBar;
    public GameObject CurrentDogmaSearchTreeProgressBar;
    
    [Header("UISettings")]
    private TextMeshProUGUI nationNameText;
    private TextMeshProUGUI populationText;
    private TextMeshProUGUI dogmaText;
    private TextMeshProUGUI foodText;
    private TextMeshProUGUI woodText;
    private TextMeshProUGUI stoneText;
    private TextMeshProUGUI metalText;
    private Slider basicSearchTreeProgressBar;
    private Slider dogmaSearchTreeProgressBar;
    private TextMeshProUGUI currentBasicSearchTreeProgressBarText;
    private TextMeshProUGUI currentDogmaSearchTreeProgressBarText;
    
    private void Awake()
    {
        nationNameText = Title.GetComponent<TextMeshProUGUI>();
        populationText = Population.GetComponent<TextMeshProUGUI>();
        dogmaText = Dogma.GetComponent<TextMeshProUGUI>();
        foodText = Food.GetComponent<TextMeshProUGUI>();
        woodText = Wood.GetComponent<TextMeshProUGUI>();
        stoneText = Stone.GetComponent<TextMeshProUGUI>();
        metalText = Metal.GetComponent<TextMeshProUGUI>();
        basicSearchTreeProgressBar = BasicSearchTreeProgressBar.GetComponent<Slider>();
        dogmaSearchTreeProgressBar = DogmaSearchTreeProgressBar.GetComponent<Slider>();
        currentBasicSearchTreeProgressBarText = CurrentBasicSearchTreeProgressBar.GetComponent<TextMeshProUGUI>();
        currentDogmaSearchTreeProgressBarText = CurrentDogmaSearchTreeProgressBar.GetComponent<TextMeshProUGUI>();
        
        NationEvents.OnNationSelected += UpdateUI;
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        NationEvents.OnNationSelected -= UpdateUI;
    }
    
    private void UpdateUI(SearchTree nation)
    {
        Debug.Log("NationIdentityRefs.UpdateUI");
        
        gameObject.SetActive(true);
        
        nationNameText.text = nation.cityUtilityAI.cityName;
        
        populationText.text = "Population : " + nation.cityUtilityAI.AgentsQuantity;
        dogmaText.text = "Dogma : " + nation.cityUtilityAI.CurrentDogma;
        foodText.text = "Food : " + nation.cityUtilityAI.TotalFood;
        woodText.text = "Wood : " + nation.cityUtilityAI.TotalWood;
        stoneText.text = "Stone : " + nation.cityUtilityAI.TotalStone;
        metalText.text = "Metal : " + nation.cityUtilityAI.TotalMetal;
        
        currentBasicSearchTreeProgressBarText.text = nation.basicTechUnlockQuantity + "/" + nation.basicTechUnlockQuantityMax;
        basicSearchTreeProgressBar.value = nation.basicTechUnlockQuantity;
        basicSearchTreeProgressBar.maxValue = nation.basicTechUnlockQuantityMax;
        
        currentDogmaSearchTreeProgressBarText.text = nation.dogmaTechUnlockQuantity + "/" + nation.dogmaTechUnlockQuantityMax;
        dogmaSearchTreeProgressBar.value = nation.dogmaTechUnlockQuantity;
        dogmaSearchTreeProgressBar.maxValue = nation.dogmaTechUnlockQuantityMax;
    }
}

public static class NationEvents
{
    public static System.Action<SearchTree> OnNationSelected;
}
