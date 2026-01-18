using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ClienteManager : MonoBehaviour
{
    [Header("Configuración de spawn")]
    public ClientesSO[] clientesDisponibles; // Lista de tipos posibles
    public float tiempoEntreClientes = 5f;

    [Header("Mesas del restaurante")]
    public Mesa[] mesas;

    [Header("Empezar Turno Counter")]
    public GameObject ETC; //Gameobject del counter de empezar turno

    [Header("NavMesh")]
    [Tooltip("Distancia a la que el agente considerará que ha llegado")]
    public float agentStoppingDistance = 0.5f;

    private List<GameObject> clientesActivos = new List<GameObject>();
    private Dictionary<GameObject, int> clienteMesa = new Dictionary<GameObject, int>();

    private bool Empezado = true; //Bool para empezar solo una vez la corrutina

    private void Update()
    {
        EmpezarTurno em = ETC.GetComponent<EmpezarTurno>(); //Referencia a empezar turno

        if (em.empezado && Empezado) //Empieza la corrutina
        {
            StartCoroutine(SpawnClientes());
            Empezado = false;
        }

        if (!em.empezado) //Se para todo
        {
            StopAllCoroutines();
            // Además, detener a los NavMeshAgents activos para que no sigan moviéndose.
            foreach (var cliente in clientesActivos)
            {
                if (cliente == null) continue;
                if (cliente.TryGetComponent<NavMeshAgent>(out NavMeshAgent a))
                {
                    a.isStopped = true;
                    a.ResetPath();
                }
            }
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
        if (mesaLibre == -1) return;

        ClientesSO data = clientesDisponibles[Random.Range(0, clientesDisponibles.Length)];
        if (data.prefab == null)
        {
            Debug.LogWarning($"El cliente {data.nombre} no tiene prefab asignado.");
            return;
        }

        GameObject nuevoCliente = Instantiate(data.prefab, transform.position, data.prefab.transform.rotation);
        clientesActivos.Add(nuevoCliente);

        // ---- Ajuste: asegurar que el visual/modelo conserve la inclinación X ----
        // Busca un child llamado "Model" por convención; si no existe, busca el primer MeshRenderer.
        Transform visual = nuevoCliente.transform.Find("Model");
        if (visual == null)
        {
            foreach (Transform t in nuevoCliente.GetComponentsInChildren<Transform>())
            {
                if (t.GetComponent<MeshRenderer>() != null)
                {
                    visual = t;
                    break;
                }
            }
        }

        if (visual != null)
        {
            // Mantén la inclinación X que necesites (ej. -90)
            visual.localEulerAngles = new Vector3(-90f, 0f, 0f);
        }

        // Ajustes iniciales del NavMeshAgent si existe
        if (nuevoCliente.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
        {
            agent.stoppingDistance = agentStoppingDistance;
            agent.autoBraking = true;
            agent.isStopped = false;
            // Deja updateRotation=true si quieres que el agente rote la raíz para mirar a la dirección de movimiento.
            agent.updateRotation = true;
        }
        else
        {
            Debug.LogWarning($"Prefab del cliente {data.nombre} no contiene NavMeshAgent. Añádelo al prefab para movimiento con NavMesh.");
        }

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
        if (cliente == null)
            yield break;

        mesas[indexMesa].ocupada = true;
        clienteMesa[cliente] = indexMesa;

        Transform destino = mesas[indexMesa].posicion;

        NavMeshAgent agent = null;
        cliente.TryGetComponent<NavMeshAgent>(out agent);

        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(destino.position);

            // Esperar hasta que llegue (o hasta que el path esté listo y remainingDistance <= stoppingDistance)
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                // Si el turno se para desde fuera, es posible que quieras abortar:
                yield return null;
                if (agent == null) yield break;
            }
        }
        else
        {
            // Fallback: movimiento simple si no hay NavMeshAgent (mantén esto para debug o prefabs sin agente)
            while (Vector3.Distance(cliente.transform.position, destino.position) > 0.1f)
            {
                cliente.transform.position = Vector3.MoveTowards(
                    cliente.transform.position,
                    destino.position,
                    Time.deltaTime * 2f
                );
                yield return null;
            }
        }

        // Llegó a la mesa: opcional ajustar rotación
        Vector3 lookDir = destino.position - cliente.transform.position;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            cliente.transform.rotation = Quaternion.LookRotation(lookDir);
        }

        // Cliente se queda un tiempo (definido en el ScriptableObject)
        // Asegúrate de que ClientesSO tenga 'tiempoEnMesa' o cambia esto
        yield return new WaitForSeconds(data.tiempoEnMesa);

        // Cliente se va
        if (clienteMesa.TryGetValue(cliente, out int idx))
        {
            if (idx >= 0 && idx < mesas.Length)
                mesas[idx].ocupada = false;
            clienteMesa.Remove(cliente);
        }

        clientesActivos.Remove(cliente);
        if (cliente != null)
            Destroy(cliente);
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

        // Detener agente si lo tiene
        if (cliente.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        clientesActivos.Remove(cliente);
        Destroy(cliente);
    }
}

