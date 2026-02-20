using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlantSelectionUI : MonoBehaviour
{
    public GameObject panel;
    public Button[] plantButtons; // 3 botones
    public Text[] buttonTexts;

    private Farming farming;
    private PlayerInventory playerInventory;
    private List<GameObject> availablePlants = new List<GameObject>();

    public void Open(Farming f, PlayerInventory inventory)
    {
        farming = f;
        playerInventory = inventory;

        availablePlants.Clear();

        foreach (var entry in inventory.GetAvailableSeeds())
        {
            if (entry.Value > 0)
                availablePlants.Add(entry.Key);
        }

        for (int i = 0; i < plantButtons.Length; i++)
        {
            if (i < availablePlants.Count)
            {
                plantButtons[i].gameObject.SetActive(true);
                buttonTexts[i].text = availablePlants[i].name + " x" + inventory.GetAvailableSeeds()[availablePlants[i]];
                int index = i;
                plantButtons[i].onClick.RemoveAllListeners();
                plantButtons[i].onClick.AddListener(() => PlantSelected(index));
            }
            else
            {
                plantButtons[i].gameObject.SetActive(false);
            }
        }

        panel.SetActive(true);
    }

    private void PlantSelected(int index)
    {
        GameObject plantPrefab = availablePlants[index];

        if (playerInventory.HasSeeds(plantPrefab))
        {
            farming.PlantSeed(plantPrefab);
            playerInventory.UseSeed(plantPrefab);
        }

        panel.SetActive(false);
    }
}