using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private bool isPaused;

    private void Start()
    {
        Time.timeScale = 1;
    }

    public void PlayButton()
    {
        LoadingScreenManager.Instance.SwitchToScene();
    }
    
    public void QuitApplication()
    {
        Application.Quit();
    }

    public void ChangePauseMode()
    {
        if (isPaused)
        {
            Time.timeScale = 1;
            isPaused = false;
        }
        else 
        {
            Time.timeScale = 0;
            isPaused = true;
        }
    }
    
    public void LoadMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
