using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ClienteManager : MonoBehaviour
{
    [Header("Configuracion de spawn")]
    public ClientesSO[] clientesDisponibles;
    public float tiempoEntreClientes = 5f;

    [Header("Mesas del restaurante")]
    public Mesa[] mesas;

    [Header("Empezar Turno Counter")]
    public GameObject ETC;

    [Header("NavMesh")]
    public float agentStoppingDistance = 0.5f;

    [Header("Sitting")]
    public float sitYOffset = 0.35f;
    public float sitLerpDuration = 0.25f;

    [Header("Chair detection & rotation")]
    [Tooltip("Radio (m) para considerar sillas cercanas al destino")]
    public float chairDetectRadius = 0.6f;

    [Tooltip("Duracion (s) para interpolar la rotacion Y al sentarse")]
    public float rotationLerpDuration = 0.12f;

    [Tooltip("Offset en grados que se suma a la rotacion de la silla tipo 'SillaDe'")]
    public float rotationOffsetY_De = 0f;

    [Tooltip("Offset en grados que se suma a la rotacion de la silla tipo 'SillaIz'")]
    public float rotationOffsetY_Iz = 0f;

    private readonly List<GameObject> clientesActivos = new List<GameObject>();
    private readonly Dictionary<GameObject, int> clienteMesa = new Dictionary<GameObject, int>();
    private readonly Dictionary<GameObject, Vector3> visualOriginalLocalPos = new Dictionary<GameObject, Vector3>();
    private readonly Dictionary<GameObject, NavMeshAgent> agentesClientes = new Dictionary<GameObject, NavMeshAgent>();
    private readonly List<Transform> sillasCacheadas = new List<Transform>();

    private EmpezarTurno empezarTurno;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        CacheEmpezarTurno();
        RefreshChairCache();
    }

    private void Update()
    {
        if (empezarTurno == null)
        {
            CacheEmpezarTurno();
            if (empezarTurno == null)
                return;
        }

        if (empezarTurno.empezado)
        {
            if (spawnCoroutine == null)
                spawnCoroutine = StartCoroutine(SpawnClientes());
        }
        else
        {
            if (spawnCoroutine != null)
                StopShiftCoroutinesAndAgents();
        }
    }

    private void CacheEmpezarTurno()
    {
        if (ETC != null)
        {
            empezarTurno = ETC.GetComponent<EmpezarTurno>();
        }

        if (empezarTurno == null)
        {
            empezarTurno = FindObjectOfType<EmpezarTurno>();
            if (empezarTurno != null)
                ETC = empezarTurno.gameObject;
        }
    }

    private IEnumerator SpawnClientes()
    {
        while (true)
        {
            yield return new WaitForSeconds(tiempoEntreClientes);
            SpawnCliente();
        }
    }

    private void SpawnCliente()
    {
        if (clientesDisponibles == null || clientesDisponibles.Length == 0)
        {
            Debug.LogWarning("[ClienteManager] No hay clientes disponibles asignados.");
            return;
        }

        int mesaLibre = BuscarMesaLibre();
        if (mesaLibre == -1)
            return;

        ClientesSO data = clientesDisponibles[Random.Range(0, clientesDisponibles.Length)];
        if (data == null)
        {
            Debug.LogWarning("[ClienteManager] Hay un cliente nulo en clientesDisponibles.");
            return;
        }

        if (data.prefab == null)
        {
            Debug.LogWarning("[ClienteManager] El cliente " + data.nombre + " no tiene prefab asignado.");
            return;
        }

        GameObject nuevoCliente = Instantiate(data.prefab, transform.position, data.prefab.transform.rotation);
        clientesActivos.Add(nuevoCliente);

        Transform visual = GetVisualTransform(nuevoCliente);
        if (visual != null)
            visual.localEulerAngles = new Vector3(-90f, 0f, 0f);

        NavMeshAgent agent = nuevoCliente.GetComponentInChildren<NavMeshAgent>();
        if (agent != null)
        {
            ConfigureAgent(agent);
            agentesClientes[nuevoCliente] = agent;
        }
        else
        {
            Debug.LogWarning("[ClienteManager] El prefab del cliente " + data.nombre + " no contiene NavMeshAgent.");
        }

        StartCoroutine(EnviarAmesa(nuevoCliente, mesaLibre));
    }

    private void ConfigureAgent(NavMeshAgent agent)
    {
        agent.stoppingDistance = agentStoppingDistance;
        agent.autoBraking = true;
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.updatePosition = true;
    }

    private int BuscarMesaLibre()
    {
        if (mesas == null)
            return -1;

        for (int i = 0; i < mesas.Length; i++)
        {
            if (!mesas[i].ocupada)
                return i;
        }

        return -1;
    }

    private IEnumerator EnviarAmesa(GameObject cliente, int indexMesa)
    {
        if (cliente == null)
            yield break;

        if (mesas == null || indexMesa < 0 || indexMesa >= mesas.Length)
            yield break;

        if (mesas[indexMesa].posicion == null)
            yield break;

        mesas[indexMesa].ocupada = true;
        clienteMesa[cliente] = indexMesa;

        Transform destino = mesas[indexMesa].posicion;
        NavMeshAgent agent = GetCachedAgent(cliente);

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.SetDestination(destino.position);

            while (agent != null && agent.enabled && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
            {
                yield return null;
            }

            if (agent != null && agent.enabled)
                agent.isStopped = true;
        }
        else
        {
            while (cliente != null && Vector3.Distance(cliente.transform.position, destino.position) > 0.1f)
            {
                cliente.transform.position = Vector3.MoveTowards(
                    cliente.transform.position,
                    destino.position,
                    Time.deltaTime * 2f
                );

                yield return null;
            }
        }

        if (cliente == null)
            yield break;

        Transform silla = FindNearestSillaByTags(destino.position, chairDetectRadius);
        if (silla != null)
        {
            float targetY = silla.eulerAngles.y;

            if (silla.CompareTag("SillaDe"))
                targetY += rotationOffsetY_De;
            else if (silla.CompareTag("SillaIz"))
                targetY += rotationOffsetY_Iz;

            yield return StartCoroutine(RotateRootToY(cliente.transform, targetY, rotationLerpDuration));
        }
        else
        {
            Vector3 lookDir = destino.position - cliente.transform.position;
            lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.001f)
            {
                float targetY = Quaternion.LookRotation(lookDir).eulerAngles.y;
                yield return StartCoroutine(RotateRootToY(cliente.transform, targetY, rotationLerpDuration));
            }
        }

        Transform visual = GetVisualTransform(cliente);
        if (visual != null)
        {
            if (!visualOriginalLocalPos.ContainsKey(cliente))
                visualOriginalLocalPos[cliente] = visual.localPosition;

            yield return StartCoroutine(SitDown(visual, sitYOffset, sitLerpDuration));
            yield return new WaitForSeconds(0.1f);
        }

        if (agent != null)
            agent.enabled = false;

        NavMeshObstacle obstacle = cliente.GetComponent<NavMeshObstacle>();
        if (obstacle != null)
            obstacle.enabled = true;

        cliente.SendMessage("OnSitted", SendMessageOptions.DontRequireReceiver);
    }

    private NavMeshAgent GetCachedAgent(GameObject cliente)
    {
        if (cliente == null)
            return null;

        if (agentesClientes.TryGetValue(cliente, out NavMeshAgent agent))
            return agent;

        agent = cliente.GetComponentInChildren<NavMeshAgent>();
        if (agent != null)
            agentesClientes[cliente] = agent;

        return agent;
    }

    private void RefreshChairCache()
    {
        sillasCacheadas.Clear();
        AddChairsWithTag("SillaDe");
        AddChairsWithTag("SillaIz");
    }

    private void AddChairsWithTag(string tagName)
    {
        try
        {
            GameObject[] sillas = GameObject.FindGameObjectsWithTag(tagName);
            for (int i = 0; i < sillas.Length; i++)
            {
                if (sillas[i] != null)
                    sillasCacheadas.Add(sillas[i].transform);
            }
        }
        catch (UnityException)
        {
            Debug.LogWarning("[ClienteManager] No existe el tag " + tagName + ". Revisa los tags de las sillas.");
        }
    }

    private Transform FindNearestSillaByTags(Vector3 pos, float radius)
    {
        if (sillasCacheadas.Count == 0)
            RefreshChairCache();

        Transform best = null;
        float bestDist = float.MaxValue;
        float radiusSqr = radius * radius;

        for (int i = 0; i < sillasCacheadas.Count; i++)
        {
            Transform silla = sillasCacheadas[i];
            if (silla == null)
                continue;

            float dSqr = (silla.position - pos).sqrMagnitude;
            if (dSqr <= radiusSqr && dSqr < bestDist)
            {
                bestDist = dSqr;
                best = silla;
            }
        }

        return best;
    }

    private IEnumerator RotateRootToY(Transform root, float targetY, float duration)
    {
        if (root == null)
            yield break;

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

    private Transform GetVisualTransform(GameObject cliente)
    {
        if (cliente == null)
            return null;

        Transform visual = cliente.transform.Find("Model");
        if (visual != null)
            return visual;

        MeshRenderer meshRenderer = cliente.GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null && meshRenderer.transform != cliente.transform)
            return meshRenderer.transform;

        SkinnedMeshRenderer skinnedRenderer = cliente.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedRenderer != null && skinnedRenderer.transform != cliente.transform)
            return skinnedRenderer.transform;

        return cliente.transform;
    }

    private IEnumerator SitDown(Transform visual, float yOffset, float duration)
    {
        if (visual == null)
            yield break;

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

    private void StopShiftCoroutinesAndAgents()
    {
        StopAllCoroutines();
        spawnCoroutine = null;

        for (int i = 0; i < clientesActivos.Count; i++)
        {
            GameObject cliente = clientesActivos[i];
            if (cliente == null)
                continue;

            NavMeshAgent agent = GetCachedAgent(cliente);
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }
    }

    public void ClienteAdios(GameObject cliente)
    {
        if (cliente == null)
            return;

        if (clienteMesa.TryGetValue(cliente, out int indexMesa))
        {
            if (mesas != null && indexMesa >= 0 && indexMesa < mesas.Length)
                mesas[indexMesa].ocupada = false;

            clienteMesa.Remove(cliente);
        }

        if (visualOriginalLocalPos.TryGetValue(cliente, out Vector3 originalPosition))
        {
            Transform visual = GetVisualTransform(cliente);
            if (visual != null)
                visual.localPosition = originalPosition;

            visualOriginalLocalPos.Remove(cliente);
        }

        NavMeshAgent agent = GetCachedAgent(cliente);
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        agentesClientes.Remove(cliente);
        clientesActivos.Remove(cliente);
        Destroy(cliente);
    }
}