using UnityEngine;

public class Bed : MonoBehaviour
{
    public GameTimeSystem gameTimeSystem;

    private void Awake()
    {
        if (gameTimeSystem == null)
            gameTimeSystem = FindObjectOfType<GameTimeSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (gameTimeSystem == null)
            gameTimeSystem = FindObjectOfType<GameTimeSystem>();

        if (gameTimeSystem != null)
        {
            gameTimeSystem.Dormir();
        }
        else
        {
            Debug.LogWarning("[Bed] No se ha encontrado GameTimeSystem en la escena.");
        }
    }
}