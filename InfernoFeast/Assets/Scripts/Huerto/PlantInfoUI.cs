using UnityEngine;
using UnityEngine.UI;

public class PlantInfoUI : MonoBehaviour
{
    public GameObject panel;
    public Text nameText;
    public Slider growthSlider;
    public Button waterButton;

    private Plant plant;

    public void Open(Plant p)
    {
        plant = p;
        panel.SetActive(true);
        nameText.text = plant.plantName;
        growthSlider.value = plant.GetGrowthProgress();

        waterButton.onClick.RemoveAllListeners();
        waterButton.onClick.AddListener(() => plant.Water());
    }

    void Update()
    {
        if (panel.activeSelf && plant != null)
            growthSlider.value = plant.GetGrowthProgress();
    }
}