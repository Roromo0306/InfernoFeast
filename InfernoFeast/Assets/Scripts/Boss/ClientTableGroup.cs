using UnityEngine;

public class ClientTableGroup : MonoBehaviour
{
    [Header("Configuración")]
    public int roundIndex = 0; // 0 = ronda 1, 1 = ronda 2, 2 = ronda 3
    public TipoIngrediente requiredDish; // el ScriptableObject del plato que quiere este cliente
    public Transform snapPoint; // transform donde el plato debe quedar (child del anchor)
    public GameObject client; // referencia para desactivar cuando se retire
    public GameObject table;  // referencia para desactivar cuando se retire

    [HideInInspector]
    public bool served = false;

    // Llamar para que desaparezca (entregado correctamente)
    public void OnServed()
    {
        served = true;
        // aquí se puede hacer anim, partículas, sonido, etc.
        if (client != null) client.SetActive(false);
        if (table != null) table.SetActive(false);

        // Si prefieres destruir, usa Destroy(gameObject);
        //Destroy(gameObject);
    }

    // Cuando la ronda finaliza y el cliente no atendido: marcar como no atendido y desactivar
    public void OnMissed()
    {
        served = false;
        if (client != null) client.SetActive(false);
        if (table != null) table.SetActive(false);
    }
}
