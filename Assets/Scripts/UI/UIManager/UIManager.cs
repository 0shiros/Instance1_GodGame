using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;
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

    public void PauseButton()
    {
        if (pauseMenu.activeSelf)
        {
            pauseMenu.SetActive(false);
            settingsMenu.SetActive(false);
        }
        else
        {
            pauseMenu.SetActive(true);
        }
    }
    
    public void LoadMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
