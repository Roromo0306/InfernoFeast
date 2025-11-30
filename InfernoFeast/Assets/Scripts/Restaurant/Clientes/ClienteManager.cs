using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class ClienteManager : MonoBehaviour
{
    [Header("Configuración de spawn")]
    public ClientesSO[] clientesDisponibles; // Lista de tipos posibles
    public float tiempoEntreClientes = 5f;

    [Header("Mesas del restaurante")]
    public Mesa[] mesas;

    [Header("Empezar Turno Counter")]
    public GameObject ETC; //Gameobject del counter de empezar turno

    private List<GameObject> clientesActivos = new List<GameObject>();
    private Dictionary<GameObject, int> clienteMesa = new Dictionary<GameObject, int>();

    private bool Empezado = true; //Bool para empezar solo una vez la corrutina

    /*//Estos son antiguos gameobjects locales que los he puesto globales
    private int mesaLibre;
    private GameObject nuevoCliente;*/

    private void Update()
    {
        EmpezarTurno em = ETC.GetComponent<EmpezarTurno>(); //Referencia a empezar turno

        if (em.empezado && Empezado) //Empieza la corrutina
        {
            StartCoroutine(SpawnClientes());
            Empezado = false;
        }
        
        if(!em.empezado) //Se para todo
        {
            StopAllCoroutines();
            Empezado = true;
        }

        
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
        clienteMesa[cliente] = indexMesa;

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

      /*  // Cliente se queda un tiempo (definido en el ScriptableObject)
        yield return new WaitForSeconds(data.tiempoEnMesa);

        // Cliente se va
        mesas[indexMesa].ocupada = false;
        clientesActivos.Remove(cliente);
        Destroy(cliente);*/
    }

    public void ClienteAdios(GameObject cliente)
    {
        if (cliente == null)
            return;

        // Primero intentamos obtener el índice guardado de la mesa
        if (clienteMesa.TryGetValue(cliente, out int indexMesa))
        {
            if (indexMesa >= 0 && indexMesa < mesas.Length)
            {
                mesas[indexMesa].ocupada = false;
            }
            clienteMesa.Remove(cliente);
        }

        clientesActivos.Remove(cliente);
        Destroy(cliente);
    }
}

