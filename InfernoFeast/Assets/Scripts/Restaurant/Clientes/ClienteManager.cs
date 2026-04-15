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
    public float agentStoppingDistance = 0.5f; //Distancia a la que el agente considerará que ha llegado

    [Header("Sitting")]
    public float sitYOffset = 0.35f; //Cuánto sube el modelo al sentarse (unidades locales)
    public float sitLerpDuration = 0.25f; //Tiempo para animar el subir/bajar al sentarse

    [Header("Chair detection & rotation")]
    [Tooltip("Radio (m) para considerar sillas cercanas al destino")]
    public float chairDetectRadius = 0.6f;
    [Tooltip("Duración (s) para interpolar la rotación Y al sentarse")]
    public float rotationLerpDuration = 0.12f;

    [Tooltip("Offset en grados que se suma a la rotación de la silla tipo 'SillaDe'")]
    public float rotationOffsetY_De = 0f;
    [Tooltip("Offset en grados que se suma a la rotación de la silla tipo 'SillaIz'")]
    public float rotationOffsetY_Iz = 0f;

    private List<GameObject> clientesActivos = new List<GameObject>();
    private Dictionary<GameObject, int> clienteMesa = new Dictionary<GameObject, int>();

    private Dictionary<GameObject, Vector3> visualOriginalLocalPos = new Dictionary<GameObject, Vector3>(); //// Guardar la localPosition original del visual para restaurarla al irse.
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
                NavMeshAgent a = cliente.GetComponentInChildren<NavMeshAgent>();
                if (a != null)
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
                if (t.GetComponent<MeshRenderer>() != null || t.GetComponent<SkinnedMeshRenderer>() != null)
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
        NavMeshAgent agent = nuevoCliente.GetComponentInChildren<NavMeshAgent>();
        if (agent != null)
        {
            agent.stoppingDistance = agentStoppingDistance;
            agent.autoBraking = true;
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.updatePosition = true;
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

        NavMeshAgent agent = cliente.GetComponentInChildren<NavMeshAgent>();

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

            // Parar agente al llegar
            agent.isStopped = true;
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

        // --- Buscar la silla por TAG cerca del destino y rotar en Y para mirar "hacia delante" de la silla ---
        Transform silla = FindNearestSillaByTags(destino.position, chairDetectRadius);
        if (silla != null)
        {
            float targetY = silla.eulerAngles.y;
            // Aplicar offset según el tag de la silla
            if (silla.CompareTag("SillaDe"))
            {
                targetY += rotationOffsetY_De;
            }
            else if (silla.CompareTag("SillaIz"))
            {
                targetY += rotationOffsetY_Iz;
            }
            yield return StartCoroutine(RotateRootToY(cliente.transform, targetY, rotationLerpDuration));
        }
        else
        {
            // Si no encuentra silla, fallback a mirar hacia el destino (solo Y) sin offset
            Vector3 lookDir = destino.position - cliente.transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                float targetY = Quaternion.LookRotation(lookDir).eulerAngles.y;
                yield return StartCoroutine(RotateRootToY(cliente.transform, targetY, rotationLerpDuration));
            }
        }

        // --- Simular sentarse: subir el visual/local model en Y ---
        Transform visual = GetVisualTransform(cliente);
        if (visual != null)
        {
            // Guardar posición local original para restaurar luego
            if (!visualOriginalLocalPos.ContainsKey(cliente))
                visualOriginalLocalPos[cliente] = visual.localPosition;

            // Subir la Y local suavemente
            yield return StartCoroutine(SitDown(visual, sitYOffset, sitLerpDuration));

            yield return new WaitForSeconds(0.1f);

            // Hacemos el cambio de Agente a Obstáculo
            if (agent != null) agent.enabled = false;

            NavMeshObstacle obstacle = cliente.GetComponent<NavMeshObstacle>();
            if (obstacle != null)
            {
                obstacle.enabled = true;
            }

            cliente.SendMessage("OnSitted", SendMessageOptions.DontRequireReceiver);
        }
    }

    // Busca la silla más cercana por TAG "SillaDe" o "SillaIz" alrededor de 'pos' dentro de 'radius'.
    Transform FindNearestSillaByTags(Vector3 pos, float radius)
    {
        List<GameObject> sillas = new List<GameObject>();
        sillas.AddRange(GameObject.FindGameObjectsWithTag("SillaDe"));
        sillas.AddRange(GameObject.FindGameObjectsWithTag("SillaIz"));

        Transform best = null;
        float bestDist = float.MaxValue;
        float radiusSqr = radius * radius;

        foreach (var go in sillas)
        {
            if (go == null) continue;
            float dSqr = (go.transform.position - pos).sqrMagnitude;
            if (dSqr <= radiusSqr && dSqr < bestDist)
            {
                bestDist = dSqr;
                best = go.transform;
            }
        }

        return best;
    }

    // Rota la raíz solo en Y hasta targetY en segundos (suave)
    IEnumerator RotateRootToY(Transform root, float targetY, float duration)
    {
        Quaternion start = root.rotation;
        Quaternion end = Quaternion.Euler(0f, targetY, 0f);

        if (duration <= 0f)
        {
            root.rotation = end;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            root.rotation = Quaternion.Slerp(start, end, k);
            yield return null;
        }
        root.rotation = end;
    }

    Transform GetVisualTransform(GameObject cliente)
    {
        // Preferencia por hijo llamado "Model" (convención)
        Transform t = cliente.transform.Find("Model");
        if (t != null) return t;

        // Si no, buscar el primer hijo con MeshRenderer o SkinnedMeshRenderer
        foreach (Transform child in cliente.GetComponentsInChildren<Transform>())
        {
            if (child == cliente.transform) continue;
            if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<SkinnedMeshRenderer>() != null)
                return child;
        }

        // Si no encuentra nada, devuelve la raíz (aunque no es ideal)
        return cliente.transform;
    }

    // Coroutine para elevar el modelo local Y suavemente (sit)
    IEnumerator SitDown(Transform visual, float yOffset, float duration)
    {
        Vector3 start = visual.localPosition;
        Vector3 target = start + new Vector3(0f, yOffset, 0f);

        if (duration <= 0f)
        {
            visual.localPosition = target;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            visual.localPosition = Vector3.Lerp(start, target, k);
            yield return null;
        }
        visual.localPosition = target;
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

        // Restaurar visual si se cambió su localPosition
        if (visualOriginalLocalPos.TryGetValue(cliente, out Vector3 orig))
        {
            Transform visual = GetVisualTransform(cliente);
            if (visual != null)
            {
                visual.localPosition = orig;
            }
            visualOriginalLocalPos.Remove(cliente);
        }

        // Detener agente si lo tiene
        NavMeshAgent a = cliente.GetComponentInChildren<NavMeshAgent>();
        if (a != null && a.isOnNavMesh)
        {
            a.isStopped = true;
            a.ResetPath();
        }

        clientesActivos.Remove(cliente);
        Destroy(cliente);
    }
}


