using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    // Start is called before the first frame update
    void Start()
    {
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
            }
            else if (creditsPanel.activeSelf)
            {
                creditsPanel.SetActive(false);
            }
        }
    }

    public void NewGame()
    {
        SceneFadeManager.Instance.LoadSceneWithFade("Restaurant");
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
