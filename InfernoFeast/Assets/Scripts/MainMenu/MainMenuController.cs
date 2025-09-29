using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private GameObject settingsPanel;
    private GameObject creditsPanel;

    // Start is called before the first frame update
    void Start()
    {
        settingsPanel = GameObject.Find("Settings Panel");
        settingsPanel.SetActive(false);

        creditsPanel = GameObject.Find("Credits Panel");
        creditsPanel.SetActive(false);
    }

    public void NewGame()
    {
        SceneManager.LoadScene("Restaurant");
    }

    public void Settings()
    {
        settingsPanel.SetActive(true);
    }

    public void Credits()
    {
        creditsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    
}
