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
    private void Start()
    {
        ultimaPos = transform.position;

        if(EmpezarTurnoCounter == null)
        {
            EmpezarTurnoCounter = GameObject.Find("EmpezarTurno");
            clienteManager = GameObject.Find("ClienteManager");
            StartCoroutine(InicioCuentaAtras());
        }
        else
        {
            Debug.LogWarning("Counter no encontrado");
        }
    }

    private void Update()
    {


        //Si se ha sentado y no lo han antendido muestra el sprite de take order
        if(Sentado && !Elegido)
        {
            comanda.sprite = ListoPedir;
            canvas.enabled = true;
        }

        //Cuenta atras para saber cuanto tiempo tarda el cliente en llevar un plato
        if (AtendidoCuent)
        {
            tiempoPasado += Time.deltaTime;
        }
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
            if (Input.GetKey(KeyCode.E)) { 

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
        em.cantidadCom[pedido]++;

        //Inicia cuenta atrás
        Atendido = true;
        AtendidoCuent = true;
    }

    //Comanda de los VIP
    private void ElegirComandaV()
    {
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        pedido = Random.Range(0, 3);

        comanda.sprite = em.NombresComandas[pedido];
        em.cantidadCom[pedido]++;

        //Inicia cuenta atrás
        Atendido = true;
        AtendidoCuent = true;
    }


    //Scrip para revisar que le traes el plato correcto
    private void RevisarComanda(GameObject Player)
    {
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        AtendidoCuent = false;

        GameObject sujetarOb = Player.transform.GetChild(2).gameObject;

        if (sujetarOb.transform.childCount <= 0)
        {
           // Debug.Log("No tiene plato");
        }
        else
        {
            GameObject Plato = sujetarOb.transform.GetChild(0).gameObject;
            
            if(Plato.name == em.NombresComandas[pedido].name) //Has acertado
            {
                Destroy(Plato);
                //Debug.Log("Has acertado");
                em.cantidadCom[pedido]--;
                
                if(tiempoPasado < 75)
                {
                    comanda.sprite = Feliz;
                    modo = 1;
                }
                else
                {
                    comanda.sprite = Neutral;
                    modo = 2;
                }
                StartCoroutine(Adios());

            }
            else //No has acertado
            {
                Destroy(Plato);
                //Debug.Log("No has acertado");
                comanda.sprite = Enfadado;
                modo = 3;
                StartCoroutine(Adios());
            }
        }
    }

    IEnumerator Adios()
    {
        ClienteManager CM = clienteManager.GetComponent<ClienteManager>(); //Referencia a cliente Manager
        VariablesFinDia();

        yield return new WaitForSeconds(3f);
        canvas.enabled = false;
        CM.ClienteAdios(this.gameObject);
    }

    //Corrutina que controla la cuenta atras hasta que se marche el cliente
    IEnumerator InicioCuentaAtras()
    {
        ClienteManager CM = clienteManager.GetComponent<ClienteManager>(); //Referencia a cliente Manager
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        float tiempo = 0f; //Creo la variable tiempo (por mi la hubiera creado directamente en el if pero daba error)

        if (!Atendido) //Si no se ha atendido al cliente el tiempo es 45
        {
            tiempo = 45f;
        }
        else //Si se ha atendido al cliente el tiempo es 90
        {
            tiempo = 90f;
            Atendido = false; 

        }

        while (tiempo > 0) //Cuenta atras
        {
            tiempo -= Time.deltaTime;

            if (Atendido) //Si se ha atendido al cliente para la corrutina y la inicia de nuevo
            {
                //Debug.Log("Se ha atendido al cliente");
                StopAllCoroutines();
                StartCoroutine(InicioCuentaAtras());
            }

            yield return null;
        }
        em.cantidadCom[pedido]--;
        VariablesFinDia();
        CM.ClienteAdios(this.gameObject);
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

}
