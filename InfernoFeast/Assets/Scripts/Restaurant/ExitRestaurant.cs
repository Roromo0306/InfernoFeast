using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitRestaurant : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mapPanel;
    public GameObject exitPanel;

    [Header("Scene Names")]
    public string marketSceneName = "Market";
    public string bossSceneName = "Boss 1";
    public string fishingLakeSceneName = "Fishing Lake";
    public string farmSceneName = "Farm";
    public string cedrikRoomSceneName = "CedrikRoom";

    private void Awake()
    {
        if (mapPanel == null)
            mapPanel = GameObject.Find("Map");

        if (exitPanel == null)
            exitPanel = GameObject.Find("Exit Restaurant Panel");
    }

    private void Start()
    {
        SetPanel(mapPanel, false);
        SetPanel(exitPanel, false);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
            SetPanel(exitPanel, true);
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Player"))
            SetPanel(exitPanel, false);
    }

    public void Yes()
    {
        SetPanel(exitPanel, false);
        SetPanel(mapPanel, true);
    }

    public void No()
    {
        SetPanel(exitPanel, false);
    }

    public void Restaurant()
    {
        SetPanel(mapPanel, false);
        SetPanel(exitPanel, false);
    }

    public void Market()
    {
        LoadScene(marketSceneName, false);
    }

    public void Boss1()
    {
        LoadScene(bossSceneName, true);
    }

    public void FishingLake()
    {
        LoadScene(fishingLakeSceneName, false);
    }

    public void Farm()
    {
        LoadScene(farmSceneName, false);
    }

    public void CedriksRoom()
    {
        LoadScene(cedrikRoomSceneName, true);
    }

    private void LoadScene(string sceneName, bool preferFade)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        SetPanel(mapPanel, false);
        SetPanel(exitPanel, false);

        if (preferFade && SceneFadeManager.Instance != null)
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