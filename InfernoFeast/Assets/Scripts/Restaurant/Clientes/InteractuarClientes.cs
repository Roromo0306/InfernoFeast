using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InteractuarClientes : MonoBehaviour
{
    public GameObject EmpezarTurnoCounter;

    public int ClienteTipo = 0; // Tipo 1 = normal. Tipo 2 = VIP.

    [Header("Canvas Propio")]
    public Image comanda;

    [Header("Cliente Manager")]
    public GameObject clienteManager;

    public bool Elegido = false;

    [Header("Estado del cliente")]
    public bool Sentado = false;
    private Vector3 ultimaPos;
    private float tiempoSinMoverse = 0f;
    private float tiempoParaSentarse = 0.2f;
    public Sprite ListoPedir;

    [Header("Reacciones")]
    public Sprite Feliz;
    public Sprite Neutral;
    public Sprite Enfadado;

    public Canvas canvas;

    private bool Atendido = false;
    private bool AtendidoCuent = false;
    private float tiempoPasado;

    [Header("Variables fin del dia")]
    public int dineroCli;
    public int reputacionCli;
    private int modo = 0;

    [HideInInspector] public int pedido = -1;

    [Header("UI Slider")]
    public Slider slider;

    [Header("Variables slider")]
    private float tiempoRestanteSalida = 0f;
    private float tiempoMaxSalida = 1f;
    private bool usandoCuentaSalida = false;

    private float tiempoMaxPedido = 75f;
    private bool usandoCuentaPedido = false;

    [Header("Gameobject plato")]
    public GameObject platoI;

    [Header("Entrega de platos")]
    [SerializeField] private bool aceptarNombreConClone = true;
    [SerializeField] private bool moverPlatoEnVezDeClonar = true;

    private EmpezarTurno empezarTurno;
    private ClienteManager clienteManagerComponent;
    private GameObject playerInRange;
    private Coroutine cuentaAtrasCoroutine;
    private Coroutine adiosCoroutine;
    private bool marchando = false;
    private bool pedidoResuelto = false;
    private bool resultadoAplicado = false;

    private void Start()
    {
        ultimaPos = transform.position;
        pedido = -1;

        CacheReferences();
        PrepareSlider();

        cuentaAtrasCoroutine = StartCoroutine(InicioCuentaAtras());
    }

    private void Update()
    {
        if (marchando)
            return;

        if (Sentado && !Elegido)
        {
            SetComandaSprite(ListoPedir);
            SetCanvasVisible(true);
        }

        if (AtendidoCuent)
            tiempoPasado += Time.deltaTime;

        UpdateSliderState();

        if (empezarTurno == null || clienteManagerComponent == null)
            CacheReferences();

        if (empezarTurno != null && !empezarTurno.empezado)
        {
            LeaveImmediately();
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
            TryInteractWithPlayer();
    }

    private void CacheReferences()
    {
        if (EmpezarTurnoCounter == null)
            EmpezarTurnoCounter = GameObject.Find("EmpezarTurno");

        if (EmpezarTurnoCounter != null && empezarTurno == null)
            empezarTurno = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        if (clienteManager == null)
            clienteManager = GameObject.Find("ClienteManager");

        if (clienteManager != null && clienteManagerComponent == null)
            clienteManagerComponent = clienteManager.GetComponent<ClienteManager>();

        if (EmpezarTurnoCounter == null)
            Debug.LogWarning("[InteractuarClientes] No se ha encontrado EmpezarTurno.");

        if (clienteManager == null)
            Debug.LogWarning("[InteractuarClientes] No se ha encontrado ClienteManager.");
    }

    private void PrepareSlider()
    {
        if (slider == null)
        {
            Debug.LogWarning("[InteractuarClientes] Slider no asignado.");
            return;
        }

        slider.minValue = 0f;
        slider.value = 0f;
        slider.gameObject.SetActive(false);
    }

    private void UpdateSliderState()
    {
        if (slider == null)
            return;

        if (AtendidoCuent || usandoCuentaPedido)
        {
            usingPedidoOnSlider();
        }
        else if (usandoCuentaSalida)
        {
            usingSalidaOnSlider();
        }
        else if (slider.gameObject.activeSelf)
        {
            slider.gameObject.SetActive(false);
        }
    }

    private void usingPedidoOnSlider()
    {
        if (slider == null)
            return;

        if (!slider.gameObject.activeSelf)
            slider.gameObject.SetActive(true);

        slider.maxValue = tiempoMaxPedido;
        float restantePedido = Mathf.Clamp(tiempoMaxPedido - tiempoPasado, 0f, tiempoMaxPedido);
        slider.value = restantePedido;
        usandoCuentaPedido = true;
    }

    private void usingSalidaOnSlider()
    {
        if (slider == null)
            return;

        if (!slider.gameObject.activeSelf)
            slider.gameObject.SetActive(true);

        slider.maxValue = tiempoMaxSalida;
        slider.value = Mathf.Clamp(tiempoRestanteSalida, 0f, tiempoMaxSalida);
    }

    public void OnSitted()
    {
        Sentado = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            playerInRange = collision.gameObject;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == playerInRange)
            playerInRange = null;
    }

    private void TryInteractWithPlayer()
    {
        if (playerInRange == null)
            return;

        if (empezarTurno == null || !empezarTurno.empezado)
            return;

        if (!Sentado)
            return;

        if (!Elegido)
        {
            if (!CanAcceptNewOrder())
                return;

            ElegirComanda();
            return;
        }

        TryDeliverPlate(playerInRange);
    }

    private bool CanAcceptNewOrder()
    {
        if (empezarTurno == null)
            return false;

        int maxOrders = Mathf.Max(1, empezarTurno.maxComandas);
        return empezarTurno.empezado && empezarTurno.comandasCount < maxOrders;
    }

    private void ElegirComanda()
    {
        if (empezarTurno == null)
            return;

        if (empezarTurno.NombresComandas == null || empezarTurno.NombresComandas.Count == 0)
        {
            Debug.LogWarning("[InteractuarClientes] No hay comandas disponibles para elegir.");
            return;
        }

        pedido = Random.Range(0, empezarTurno.NombresComandas.Count);
        Sprite spritePedido = empezarTurno.NombresComandas[pedido];

        if (spritePedido == null)
        {
            Debug.LogWarning("[InteractuarClientes] La comanda elegida no tiene sprite.");
            pedido = -1;
            return;
        }

        EnsureCantidadComSize();

        SetComandaSprite(spritePedido);
        empezarTurno.ComandaUI(spritePedido.name);

        int maxOrders = Mathf.Max(1, empezarTurno.maxComandas);
        empezarTurno.comandasCount = Mathf.Clamp(empezarTurno.comandasCount + 1, 0, maxOrders);

        if (empezarTurno.cantidadCom != null && pedido >= 0 && pedido < empezarTurno.cantidadCom.Count)
            empezarTurno.cantidadCom[pedido]++;

        Elegido = true;
        pedidoResuelto = false;
        resultadoAplicado = false;

        Atendido = true;
        AtendidoCuent = true;
        tiempoPasado = 0f;

        tiempoMaxPedido = 100f;
        usandoCuentaPedido = true;

        if (slider != null)
        {
            slider.gameObject.SetActive(true);
            slider.maxValue = tiempoMaxPedido;
            slider.value = tiempoMaxPedido;
        }
    }

    private void TryDeliverPlate(GameObject player)
    {
        if (pedidoResuelto)
            return;

        if (player == null || empezarTurno == null || pedido < 0)
            return;

        if (empezarTurno.NombresComandas == null || pedido >= empezarTurno.NombresComandas.Count || empezarTurno.NombresComandas[pedido] == null)
            return;

        GameObject heldPlate = GetHeldPlate(player);
        if (heldPlate == null)
            return;

        AtendidoCuent = false;
        usandoCuentaPedido = false;

        string expectedName = empezarTurno.NombresComandas[pedido].name;
        bool correctPlate = PlateNameMatches(heldPlate.name, expectedName);

        PutPlateOnTable(heldPlate);
        RemovePendingOrder();
        pedidoResuelto = true;
        pedido = -1;

        if (correctPlate)
        {
            if (tiempoPasado < tiempoMaxPedido)
            {
                SetComandaSprite(Feliz);
                modo = 1;
            }
            else
            {
                SetComandaSprite(Neutral);
                modo = 2;
            }
        }
        else
        {
            SetComandaSprite(Enfadado);
            modo = 3;
        }

        FadeOut(2.5f);
        BeginLeavingAfterDelay(3f);
    }

    private bool PlateNameMatches(string plateName, string expectedName)
    {
        string cleanPlateName = aceptarNombreConClone ? NormalizeName(plateName) : plateName;
        string cleanExpectedName = aceptarNombreConClone ? NormalizeName(expectedName) : expectedName;
        return cleanPlateName == cleanExpectedName;
    }

    private string NormalizeName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
            return string.Empty;

        return rawName.Replace("(Clone)", string.Empty).Trim();
    }

    private GameObject GetHeldPlate(GameObject player)
    {
        if (player == null || player.transform.childCount <= 2)
            return null;

        Transform holdPoint = player.transform.GetChild(2);
        if (holdPoint == null || holdPoint.childCount <= 0)
            return null;

        return holdPoint.GetChild(0).gameObject;
    }

    private void PutPlateOnTable(GameObject plate)
    {
        if (plate == null || platoI == null)
            return;

        if (moverPlatoEnVezDeClonar)
        {
            MoveObjectToParent(plate, platoI.transform, platoI.transform.position, platoI.transform.rotation);
            plate.transform.localPosition = Vector3.zero;
            plate.transform.localRotation = Quaternion.identity;
            return;
        }

        GameObject plateCopy = Instantiate(plate, platoI.transform.position, platoI.transform.rotation);
        plateCopy.name = NormalizeName(plate.name);
        plateCopy.transform.SetParent(platoI.transform, true);
        plateCopy.transform.localPosition = Vector3.zero;
        plateCopy.transform.localRotation = Quaternion.identity;
        PrepareRigidbody(plateCopy);

        Destroy(plate);
    }

    private void MoveObjectToParent(GameObject objectToMove, Transform newParent, Vector3 targetWorldPosition, Quaternion targetWorldRotation)
    {
        if (objectToMove == null || newParent == null)
            return;

        Vector3 originalWorldScale = objectToMove.transform.lossyScale;

        objectToMove.transform.SetParent(newParent, true);
        objectToMove.transform.position = targetWorldPosition;
        objectToMove.transform.rotation = targetWorldRotation;
        SetWorldScale(objectToMove.transform, originalWorldScale);

        PrepareRigidbody(objectToMove);
    }

    private void PrepareRigidbody(GameObject target)
    {
        if (target == null)
            return;

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb == null)
            return;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void SetWorldScale(Transform target, Vector3 worldScale)
    {
        if (target == null)
            return;

        if (target.parent == null)
        {
            target.localScale = worldScale;
            return;
        }

        Vector3 parentScale = target.parent.lossyScale;

        target.localScale = new Vector3(
            parentScale.x != 0f ? worldScale.x / parentScale.x : worldScale.x,
            parentScale.y != 0f ? worldScale.y / parentScale.y : worldScale.y,
            parentScale.z != 0f ? worldScale.z / parentScale.z : worldScale.z
        );
    }

    private void RemovePendingOrder()
    {
        if (empezarTurno == null || pedido < 0)
            return;

        EnsureCantidadComSize();

        if (empezarTurno.cantidadCom != null && pedido < empezarTurno.cantidadCom.Count)
            empezarTurno.cantidadCom[pedido] = Mathf.Max(0, empezarTurno.cantidadCom[pedido] - 1);

        if (empezarTurno.NombresComandas != null && pedido < empezarTurno.NombresComandas.Count && empezarTurno.NombresComandas[pedido] != null)
            empezarTurno.EliminarComanda(empezarTurno.NombresComandas[pedido].name);

        empezarTurno.comandasCount = Mathf.Max(0, empezarTurno.comandasCount - 1);
    }

    private void EnsureCantidadComSize()
    {
        if (empezarTurno == null)
            return;

        if (empezarTurno.cantidadCom == null)
            return;

        int targetCount = empezarTurno.NombresComandas != null ? empezarTurno.NombresComandas.Count : 0;

        while (empezarTurno.cantidadCom.Count < targetCount)
            empezarTurno.cantidadCom.Add(0);

        while (empezarTurno.cantidadCom.Count > targetCount)
            empezarTurno.cantidadCom.RemoveAt(empezarTurno.cantidadCom.Count - 1);
    }

    private IEnumerator InicioCuentaAtras()
    {
        while (true)
        {
            float tiempo = Atendido ? 120f : 100f;
            Atendido = false;

            float contador = tiempo;
            tiempoMaxSalida = tiempo;
            tiempoRestanteSalida = contador;
            usandoCuentaSalida = true;

            while (contador > 0f)
            {
                contador -= Time.deltaTime;
                tiempoRestanteSalida = contador;

                if (contador <= 2f)
                    FadeOut(0.5f);

                if (Atendido)
                {
                    usandoCuentaSalida = false;
                    break;
                }

                yield return null;
            }

            if (Atendido)
                continue;

            if (pedido >= 0 && !pedidoResuelto)
                RemovePendingOrder();

            VariablesFinDia();

            if (slider != null)
                slider.gameObject.SetActive(false);

            NotifyManagerAndDestroy();
            yield break;
        }
    }

    private void VariablesFinDia()
    {
        if (resultadoAplicado)
            return;

        if (empezarTurno == null)
            return;

        switch (modo)
        {
            case 1:
                dineroCli = 2;
                reputacionCli = 2;
                break;

            case 2:
                dineroCli = 2;
                reputacionCli = 1;
                break;

            case 3:
                dineroCli = -1;
                reputacionCli = 1;
                break;

            default:
                dineroCli = 0;
                reputacionCli = -2;
                break;
        }

        empezarTurno.dineroTurno += dineroCli;
        empezarTurno.reputacionTurno += reputacionCli;

        resultadoAplicado = true;
        modo = 0;
    }

    private void FadeOut(float duracion)
    {
        if (comanda == null)
            return;

        StartCoroutine(FadeO(0f, duracion));
    }

    private IEnumerator FadeO(float target, float time)
    {
        if (comanda == null)
            yield break;

        float start = comanda.color.a;

        if (time <= 0f)
        {
            SetComandaAlpha(target);
            yield break;
        }

        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            float a = Mathf.Lerp(start, target, t / time);
            SetComandaAlpha(a);
            yield return null;
        }

        SetComandaAlpha(target);
    }

    private void SetComandaAlpha(float alpha)
    {
        if (comanda == null)
            return;

        Color color = comanda.color;
        color.a = alpha;
        comanda.color = color;
    }

    private void SetComandaSprite(Sprite sprite)
    {
        if (comanda == null || sprite == null)
            return;

        comanda.sprite = sprite;
        SetComandaAlpha(1f);
    }

    private void SetCanvasVisible(bool visible)
    {
        if (canvas != null)
            canvas.enabled = visible;
    }

    private void BeginLeavingAfterDelay(float delay)
    {
        if (marchando)
            return;

        marchando = true;
        VariablesFinDia();

        if (slider != null)
            slider.gameObject.SetActive(false);

        if (cuentaAtrasCoroutine != null)
        {
            StopCoroutine(cuentaAtrasCoroutine);
            cuentaAtrasCoroutine = null;
        }

        adiosCoroutine = StartCoroutine(Adios(delay));
    }

    private IEnumerator Adios(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetCanvasVisible(false);
        NotifyManagerAndDestroy();
    }

    private void LeaveImmediately()
    {
        if (marchando)
            return;

        marchando = true;
        StopAllCoroutines();
        NotifyManagerAndDestroy();
    }

    private void NotifyManagerAndDestroy()
    {
        if (clienteManagerComponent == null)
            CacheReferences();

        if (clienteManagerComponent != null)
        {
            clienteManagerComponent.ClienteAdios(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}