using UnityEngine;

public class NPCReward : MonoBehaviour
{
    [Header("Recompensas de semillas")]
    public GameObject[] plantPrefabs; // Plantas que puede dar
    public int seedsPerPlant = 3;

    public PlayerInventory playerInventory; // Referencia al inventario del jugador

    // Llamar al final del diálogo para dar semillas
    public void GiveSeeds()
    {
        if (playerInventory == null) return;

        foreach (var plant in plantPrefabs)
        {
            playerInventory.AddSeeds(plant, seedsPerPlant);
        }

        Debug.Log("Semillas entregadas al jugador");
    }
}