using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClienteManager : MonoBehaviour
{
    [Header("Configuración de spawn")]
    public ClientesSO[] clientesDisponibles; // Lista de tipos posibles
    public float tiempoEntreClientes = 5f;

    [Header("Mesas del restaurante")]
    public Mesa[] mesas;

    private List<GameObject> clientesActivos = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(SpawnClientes());
    }

    IEnumerator SpawnClientes()
    {
        while (true)
        {
            yield return new WaitForSeconds(tiempoEntreClientes);
            SpawnCliente();
        }
    }

    void SpawnCliente()
    {
        int mesaLibre = BuscarMesaLibre();
        if (mesaLibre == -1)
        {
            //Debug.Log("No hay mesas libres, no entra más gente.");
            return;
        }

        // Escoger un tipo de cliente aleatorio
        ClientesSO data = clientesDisponibles[Random.Range(0, clientesDisponibles.Length)];
        if (data.prefab == null)
        {
            Debug.LogWarning($"El cliente {data.nombre} no tiene prefab asignado.");
            return;
        }

        GameObject nuevoCliente = Instantiate(data.prefab, transform.position, Quaternion.identity);
        clientesActivos.Add(nuevoCliente);

        // Enviar a mesa con los datos del ScriptableObject
        StartCoroutine(EnviarAmesa(nuevoCliente, mesaLibre, data));
    }

    int BuscarMesaLibre()
    {
        for (int i = 0; i < mesas.Length; i++)
        {
            if (!mesas[i].ocupada)
                return i;
        }
        return -1;
    }

    IEnumerator EnviarAmesa(GameObject cliente, int indexMesa, ClientesSO data)
    {
        mesas[indexMesa].ocupada = true;
        Transform destino = mesas[indexMesa].posicion;

        // Movimiento simple hacia la mesa
        while (Vector3.Distance(cliente.transform.position, destino.position) > 0.1f)
        {
            cliente.transform.position = Vector3.MoveTowards(
                cliente.transform.position,
                destino.position,
                Time.deltaTime * 2f
            );
            yield return null;
        }

        // Cliente se queda un tiempo (definido en el ScriptableObject)
        yield return new WaitForSeconds(data.tiempoEnMesa);

        // Cliente se va
        mesas[indexMesa].ocupada = false;
        clientesActivos.Remove(cliente);
        Destroy(cliente);
    }
}

