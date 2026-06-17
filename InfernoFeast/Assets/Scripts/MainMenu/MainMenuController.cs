using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    [Header("Scenes")]
    public string restaurantSceneName = "Restaurant";

    private void Start()
    {
        SetPanel(settingsPanel, false);
        SetPanel(creditsPanel, false);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            SetPanel(settingsPanel, false);
            return;
        }

        if (creditsPanel != null && creditsPanel.activeSelf)
        {
            SetPanel(creditsPanel, false);
        }
    }

    public void NewGame()
    {
        LoadScene(restaurantSceneName);
    }

    public void Settings()
    {
        SetPanel(settingsPanel, true);
        SetPanel(creditsPanel, false);
    }

    public void Credits()
    {
        SetPanel(creditsPanel, true);
        SetPanel(settingsPanel, false);
    }

    public void ClosePanels()
    {
        SetPanel(settingsPanel, false);
        SetPanel(creditsPanel, false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        if (SceneFadeManager.Instance != null)
            SceneFadeManager.Instance.LoadSceneWithFade(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}