using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NationIdentity : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] GameObject canva;
    [SerializeField] GameObject prefabNationIdentity;
    private NationIdentityRefs nationIdentityRefs;
    
    [Header("NationSettings")]
    [SerializeField] string nationName;
    
    [Header("UISettings")]
    private TextMeshProUGUI nationNameText;
    private TextMeshProUGUI populationText;
    private TextMeshProUGUI dogmaText;
    private Slider basicSearchTreeProgressBar;
    private Slider dogmaSearchTreeProgressBar;
    private TextMeshProUGUI currentBasicSearchTreeProgressBarText;
    private TextMeshProUGUI currentDogmaSearchTreeProgressBarText;

    private void Start()
    {
        nationIdentityRefs = Instantiate(prefabNationIdentity, canva.transform).GetComponent<NationIdentityRefs>();
        nationNameText = nationIdentityRefs.Title.GetComponent<TextMeshProUGUI>();
        populationText = nationIdentityRefs.Population.GetComponent<TextMeshProUGUI>();
        dogmaText = nationIdentityRefs.Dogma.GetComponent<TextMeshProUGUI>();
        basicSearchTreeProgressBar = nationIdentityRefs.BasicSearchTreeProgressBar.GetComponent<Slider>();
        dogmaSearchTreeProgressBar = nationIdentityRefs.DogmaSearchTreeProgressBar.GetComponent<Slider>();
        currentBasicSearchTreeProgressBarText = nationIdentityRefs.CurrentBasicSearchTreeProgressBar.GetComponent<TextMeshProUGUI>();
        currentDogmaSearchTreeProgressBarText = nationIdentityRefs.CurrentDogmaSearchTreeProgressBar.GetComponent<TextMeshProUGUI>();
    }

    public void SetTitle()
    {
        gameObject.name = "Nation of " + nationName;
        nationIdentityRefs.gameObject.name = nationName + " Nation Identity";
        nationNameText.text = nationName;
    }

    public void SetPopulation(int pAgentsQuantity)
    {
        populationText.text = "Population : " + pAgentsQuantity;
    }

    public void SetDogma(E_Dogma pCurrentDogma)
    {
        dogmaText.text = "Dogma : " + pCurrentDogma;
    }

    public void SetBasicSearchTreeProgressBar(int pBasicTechUnlockQuantity, int pBasicTechUnlockQuantityMax)
    {
       currentBasicSearchTreeProgressBarText.text = pBasicTechUnlockQuantity + "/" + pBasicTechUnlockQuantityMax;
       basicSearchTreeProgressBar.value = pBasicTechUnlockQuantity;
       basicSearchTreeProgressBar.maxValue = pBasicTechUnlockQuantityMax;
    }
    
    public void SetDogmaSearchTreeProgressBar(int pDogmaTechUnlockQuantity, int pDogmaTechUnlockQuantityMax)
    {
       currentDogmaSearchTreeProgressBarText.text = pDogmaTechUnlockQuantity + "/" + pDogmaTechUnlockQuantityMax;
       dogmaSearchTreeProgressBar.value = pDogmaTechUnlockQuantity;
       dogmaSearchTreeProgressBar.maxValue = pDogmaTechUnlockQuantityMax;
    }
}
