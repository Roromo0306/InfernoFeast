using UnityEngine;

public class Bed : MonoBehaviour
{
    public GameTimeSystem gameTimeSystem;

    private void Awake()
    {
        if (gameTimeSystem == null)
            gameTimeSystem = GameTimeSystem.Instance != null ? GameTimeSystem.Instance : FindObjectOfType<GameTimeSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (gameTimeSystem == null)
            gameTimeSystem = GameTimeSystem.Instance != null ? GameTimeSystem.Instance : FindObjectOfType<GameTimeSystem>();

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