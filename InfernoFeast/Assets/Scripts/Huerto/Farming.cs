using UnityEngine;

public class Farming : MonoBehaviour
{
    public Transform spawnPoint;
    public Plant currentPlant;

    public PlantSelectionUI selectionUI;
    public PlantInfoUI infoUI;

    // Interacción con inventario opcional
    public void Interact(PlayerInventory playerInventory = null)
    {
        if (currentPlant == null && playerInventory != null)
        {
            selectionUI.Open(this, playerInventory);
        }
        else if (currentPlant != null)
        {
            infoUI.Open(currentPlant);
        }
    }

    public void PlantSeed(GameObject plantPrefab)
    {
        if (currentPlant != null) return;

        GameObject plantObj = Instantiate(plantPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);
        currentPlant = plantObj.GetComponent<Plant>();
    }
}