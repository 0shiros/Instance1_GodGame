using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;
    public GameObject LoadingScreen;
    public Slider LoadingSlider;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SwitchToScene()
    {
        LoadingScreen.SetActive(true);
        LoadingSlider.value = 0f;
        StartCoroutine(SwitchToSceneAsync());
    }
    
    IEnumerator SwitchToSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);

        while (!asyncLoad.isDone)
        {
            LoadingSlider.value = asyncLoad.progress;
            yield return null;
        }
        
        yield return new WaitForSeconds(0.2f);
        LoadingScreen.SetActive(false);
    }
}
