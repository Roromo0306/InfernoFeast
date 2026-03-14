using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractuarClientes : MonoBehaviour
{
    public GameObject EmpezarTurnoCounter;

    public int ClienteTipo = 0; //Tipo 1 = normal. Tipo 2 = VIP;

    [Header("Canvas Propio")]
    public Image comanda;

    [Header("Cliente Manager")]
    public GameObject clienteManager;

    public bool Elegido = false;

    //Variables para controlar si se ha sentado
    public bool Sentado = false; //Bool para saber si se ha sentado
    private Vector3 ultimaPos;
    private float tiempoSinMoverse = 0f; //Calcula el tiempo que esta sin moverse
    private float tiempoParaSentarse = 0.2f; //Tiempo de referencia para saber si se ha sentado
    public Sprite ListoPedir; //Sprite que indica que está listo para pedir

    [Header("Reacciones")]
    public Sprite Feliz;
    public Sprite Neutral;
    public Sprite Enfadado;

    public Canvas canvas;

    private bool Atendido = false; //Este bool controlara si se ha atendido al cliente.

    private bool AtendidoCuent = false;
    private float tiempoPasado;

    [Header("Variables fin del dia")]
    public int dineroCli;
    public int reputacionCli;
    private int modo = 0; //El modo indica la recompensa que se obtendra del cliente. Leyenda: 0-Nunca llego el plato o nunca se atendio al cliente, 1-Plato correcto y a tiempo, 2-Plato correcto pero a destiempo, 3-Plato incorrecto

    [HideInInspector] public int pedido;

    [Header("UI Slider")]
    public Slider slider; //Referencia al slider

    // --- NUEVAS VARIABLES para sincronizar slider con las cuentas ---
    private float tiempoRestanteSalida = 0f; // tiempo restante para que se marche el cliente (se actualiza desde InicioCuentaAtras)
    private float tiempoMaxSalida = 1f; // max del slider para la salida
    private bool usandoCuentaSalida = false;

    private float tiempoMaxPedido = 75f; // límite que usas para decidir "a tiempo" / "tarde"
    private bool usandoCuentaPedido = false; // true mientras AtendidoCuent == true (pedido activo)
    // -----------------------------------------------------------------

    private void Start()
    {
        ultimaPos = transform.position;

        if (EmpezarTurnoCounter == null)
        {
            EmpezarTurnoCounter = GameObject.Find("EmpezarTurno");
        }

        if (clienteManager == null)
        {
            clienteManager = GameObject.Find("ClienteManager");
        }

        if (EmpezarTurnoCounter == null)
        {
            Debug.LogWarning("Counter no encontrado");
        }

        // Slider: asegurarnos de que tenga valores razonables
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.value = 0f;
            slider.gameObject.SetActive(false); // oculto por defecto
        }
        else
        {
            Debug.LogWarning("Slider no asignado en InteractuarClientes.");
        }

        StartCoroutine(InicioCuentaAtras());
    }

    private void Update()
    {
        //Si se ha sentado y no lo han antendido muestra el sprite de take order
        if (Sentado && !Elegido)
        {
            comanda.sprite = ListoPedir;
            canvas.enabled = true;
        }

        //Cuenta atras para saber cuanto tiempo tarda el cliente en llevar un plato (reloj "pedido")
        if (AtendidoCuent)
        {
            tiempoPasado += Time.deltaTime;
        }

        // --- Actualización del slider cada frame ---
        if (slider == null) return;

        // Prioridad: mostrar timer de pedido si está activo
        if (AtendidoCuent || usandoCuentaPedido)
        {
            usingPedidoOnSlider();
        }
        else if (usandoCuentaSalida)
        {
            usingSalidaOnSlider();
        }
        else
        {
            // Si no hay ninguna cuenta activa, ocultamos slider
            if (slider.gameObject.activeSelf) slider.gameObject.SetActive(false);
        }
    }

    private void usingPedidoOnSlider()
    {
        // Habilitar slider
        if (!slider.gameObject.activeSelf) slider.gameObject.SetActive(true);

        // Establecemos max y valor según tiempo de pedido
        slider.maxValue = tiempoMaxPedido;
        float restantePedido = Mathf.Clamp(tiempoMaxPedido - tiempoPasado, 0f, tiempoMaxPedido);
        slider.value = restantePedido;
        usandoCuentaPedido = true;
    }

    private void usingSalidaOnSlider()
    {
        if (!slider.gameObject.activeSelf) slider.gameObject.SetActive(true);

        slider.maxValue = tiempoMaxSalida;
        slider.value = Mathf.Clamp(tiempoRestanteSalida, 0f, tiempoMaxSalida);
    }

    public void OnSitted()
    {
        Sentado = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        if (em.empezado)
        {
            if (Input.GetKey(KeyCode.E))
            {

                //Clientes normales cuando no se les ha atendido
                if (collision.gameObject.CompareTag("Player") && ClienteTipo == 1 && !Elegido && Sentado)
                {
                    ElegirComandaN();
                    Elegido = true;
                }

                //Clientes VIP cuando no se les ha atendido
                if (collision.gameObject.CompareTag("Player") && ClienteTipo == 2 && !Elegido && Sentado)
                {
                    ElegirComandaV();
                    Elegido = true;
                }

                //Clientes normales atendidos
                if (collision.gameObject.CompareTag("Player") && ClienteTipo == 1 && Elegido && Sentado)
                {
                    GameObject Colisionado = collision.gameObject;
                    RevisarComanda(Colisionado);
                }


                //Clientes VIP atendidos
                if (collision.gameObject.CompareTag("Player") && ClienteTipo == 2 && Elegido && Sentado)
                {
                    GameObject Colisionado = collision.gameObject;
                    RevisarComanda(Colisionado);
                }

            }
        }
    }

    //Comanda de los clientes nomales
    private void ElegirComandaN()
    {
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        pedido = Random.Range(0, 3);

        comanda.sprite = em.NombresComandas[pedido];
        em.ComandaUI(em.NombresComandas[pedido].name);
        em.cantidadCom[pedido]++;

        //Inicia cuenta atrás (pedido)
        Atendido = true;
        AtendidoCuent = true;
        tiempoPasado = 0f;

        // Activamos la cuenta del pedido en el slider
        tiempoMaxPedido = 75f; // si quieres que sea otro valor, cámbialo aquí
        usandoCuentaPedido = true;
        if (slider != null) { slider.gameObject.SetActive(true); slider.maxValue = tiempoMaxPedido; slider.value = tiempoMaxPedido; }
    }

    //Comanda de los VIP
    private void ElegirComandaV()
    {
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        pedido = Random.Range(0, 3);

        comanda.sprite = em.NombresComandas[pedido];
        em.ComandaUI(em.NombresComandas[pedido].name);

        em.cantidadCom[pedido]++;

        //Inicia cuenta atrás (pedido)
        Atendido = true;
        AtendidoCuent = true;
        tiempoPasado = 0f;

        // Activamos la cuenta del pedido en el slider
        tiempoMaxPedido = 75f;
        usandoCuentaPedido = true;
        if (slider != null) { slider.gameObject.SetActive(true); slider.maxValue = tiempoMaxPedido; slider.value = tiempoMaxPedido; }
    }


    //Scrip para revisar que le traes el plato correcto
    private void RevisarComanda(GameObject Player)
    {
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        AtendidoCuent = false;
        usandoCuentaPedido = false; // parar la visualización del timer de pedido

        GameObject sujetarOb = Player.transform.GetChild(2).gameObject;

        if (sujetarOb.transform.childCount <= 0)
        {
            // Debug.Log("No tiene plato");
        }
        else
        {
            GameObject Plato = sujetarOb.transform.GetChild(0).gameObject;

            if (Plato.name == em.NombresComandas[pedido].name) //Has acertado
            {
                Destroy(Plato);
                //Debug.Log("Has acertado");
                em.cantidadCom[pedido]--;

                if (tiempoPasado < 75)
                {
                    comanda.sprite = Feliz;
                    modo = 1;
                }
                else
                {
                    comanda.sprite = Neutral;
                    modo = 2;
                }
                em.PosicionEntregarPlato();
                FadeOut(0.5f); //Activamos el fade out
                StartCoroutine(Adios());

            }
            else //No has acertado
            {
                Destroy(Plato);
                //Debug.Log("No has acertado");
                comanda.sprite = Enfadado; //Sprite de enfadado
                modo = 3;
                em.PosicionEntregarPlato();
                FadeOut(0.5f); //Activamos el fade out
                StartCoroutine(Adios()); //Corrutina que finaliza todo
            }
        }
    }

    IEnumerator Adios()
    {
        ClienteManager CM = clienteManager.GetComponent<ClienteManager>(); //Referencia a cliente Manager
        VariablesFinDia();

        // ocultamos slider cuando se vaya del cliente (espera 3s y se marcha)
        if (slider != null) slider.gameObject.SetActive(false);

        yield return new WaitForSeconds(3f);
        canvas.enabled = false;
        CM.ClienteAdios(this.gameObject);
    }

    //Corrutina que controla la cuenta atras hasta que se marche el cliente
    IEnumerator InicioCuentaAtras()
    {
        ClienteManager CM = clienteManager.GetComponent<ClienteManager>(); //Referencia a cliente Manager
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        while (true)
        {
            // Determina duración según si se ha atendido
            float tiempo = Atendido ? 90f : 45f;
            // Consumimos el flag Atendido (lo usamos para decidir la duración)
            Atendido = false;

            float contador = tiempo;

            // Preparamos valores para el slider (cuenta de salida)
            tiempoMaxSalida = tiempo;
            tiempoRestanteSalida = contador;
            usandoCuentaSalida = true;

            // Loop de cuenta atrás
            while (contador > 0f)
            {
                contador -= Time.deltaTime;
                tiempoRestanteSalida = contador; // actualizamos la variable visible

                if (contador <= 2f)
                {
                    FadeOut(0.5f);
                }

                // Si se atiende de nuevo mientras contamos, salimos para reiniciar el bucle
                if (Atendido)
                {
                    // Se ha atendido (nuevo pedido), reiniciamos la cuenta desde el principio
                    usandoCuentaSalida = false; // dejamos de mostrar la cuenta de salida
                    break;
                }

                yield return null;
            }

            // Si Atendido está true significa que se reinició por un nuevo pedido -> continuamos el while para recalcular tiempo
            if (Atendido)
            {
                continue;
            }

            // Llegamos aquí sólo si se agotó el tiempo y no se atendió
            if (pedido >= 0 && em != null)
            {
                if (pedido < em.cantidadCom.Count)
                    em.cantidadCom[pedido]--;
            }

            // El cliente se marcha por tiempo agotado
            VariablesFinDia();

            // ocultamos slider antes de avisar manager
            if (slider != null) slider.gameObject.SetActive(false);

            CM.ClienteAdios(this.gameObject);
            yield break; // terminamos la corrutina del cliente (se marcha)
        }
    }

    private void VariablesFinDia()
    {
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        if (modo == 0)
        {
            dineroCli = 0;
            reputacionCli = -2;
        }

        if (modo == 1)
        {
            dineroCli = 2;
            reputacionCli = 2;
        }

        if (modo == 2)
        {
            dineroCli = 2;
            reputacionCli = 1;

        }

        if (modo == 3)
        {
            dineroCli = -1;
            reputacionCli = 1;
        }

        //Se annade el dinero y la reputacion al manager de turno
        em.dineroTurno += dineroCli;
        em.reputacionTurno += reputacionCli;

        modo = 0;
    }

    //Funicones relacionadas con el fade out
    private void FadeOut(float duracion)
    {
        StartCoroutine(FadeO(0f, duracion));
    }

    IEnumerator FadeO(float target, float time)
    {
        float start = comanda.color.a;

        for (float t = 0; t < time; t += Time.deltaTime)
        {
            float a = Mathf.Lerp(start, target, t / time);
            comanda.color = new Color(comanda.color.r, comanda.color.g, comanda.color.b, a);
            yield return null;
        }
        comanda.color = new Color(comanda.color.r, comanda.color.g, comanda.color.b, target);
    }
}
