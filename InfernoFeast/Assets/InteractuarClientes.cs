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

    public Canvas canvas;


    private bool Atendido = false; //Este bool controlara si se ha atendido al cliente.
    [HideInInspector] public int pedido;
    private void Start()
    {
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

    private void OnCollisionEnter(Collision collision)
    {
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        if (em.empezado)
        { 
            //Clientes normales cuando no se les ha atendido
            if (collision.gameObject.CompareTag("Player") && ClienteTipo == 1 && !Elegido) 
            {
                ElegirComandaN();
                Elegido = true;
            }

            //Clientes VIP cuando no se les ha atendido
            if (collision.gameObject.CompareTag("Player") && ClienteTipo == 2 && !Elegido)
            {
                ElegirComandaV();
                Elegido = true;
            }

            //Clientes normales atendidos
            if (collision.gameObject.CompareTag("Player") && ClienteTipo == 1 && Elegido)
            {
                GameObject Colisionado = collision.gameObject;
                RevisarComanda(Colisionado);
            }


            //Clientes VIP atendidos
            if (collision.gameObject.CompareTag("Player") && ClienteTipo == 2 && Elegido)
            {
                GameObject Colisionado = collision.gameObject;
                RevisarComanda(Colisionado);
            }
        }
    }
    
    //Comanda de los clientes nomales
    private void ElegirComandaN()
    {
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        pedido = Random.Range(0, 3);

        comanda.sprite = em.NombresComandas[pedido];
        canvas.enabled = true;
        em.cantidadCom[pedido]++;

        //Inicia cuenta atrás
        float tiempo = 60f;
        Atendido = true;
    }

    //Comanda de los VIP
    private void ElegirComandaV()
    {
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        pedido = Random.Range(0, 3);

        comanda.sprite = em.NombresComandas[pedido];
        em.cantidadCom[pedido]++;

        canvas.enabled = true;

        //Inicia cuenta atrás
        float tiempo = 60f;
        Atendido = true;
    }


    //Scrip para revisar que le traes el plato correcto
    private void RevisarComanda(GameObject Player)
    {
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();
        ClienteManager CM = clienteManager.GetComponent<ClienteManager>(); //Referencia a cliente Manager

        GameObject sujetarOb = Player.transform.GetChild(2).gameObject;

        if (sujetarOb.transform.childCount <= 0)
        {
            Debug.Log("No tiene plato");
        }
        else
        {
            GameObject Plato = sujetarOb.transform.GetChild(0).gameObject;
            
            if(Plato.name == em.NombresComandas[pedido].name)
            {
                Destroy(Plato);
                Debug.Log("Has acertado");
                em.cantidadCom[pedido]--;

                canvas.enabled = false;
                CM.ClienteAdios(this.gameObject);
            }
            else
            {
                Destroy(Plato);
                Debug.Log("No has acertado");
                canvas.enabled = false;
                CM.ClienteAdios(this.gameObject);
            }
        }
    }


    //Corrutina que controla la cuenta atras hasta que se marche el cliente
    IEnumerator InicioCuentaAtras()
    {
        ClienteManager CM = clienteManager.GetComponent<ClienteManager>(); //Referencia a cliente Manager
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        float tiempo = 0f; //Creo la variable tiempo (por mi la hubiera creado directamente en el if pero daba error)

        if (!Atendido) //Si no se ha atendido al cliente el tiempo es 45
        {
            tiempo = 90f;
        }
        else //Si se ha atendido al cliente el tiempo es 60
        {
            tiempo = 90f;
            Atendido = false;
        }

        while (tiempo > 0) //Cuenta atras
        {
            tiempo -= Time.deltaTime;

            if (Atendido) //Si se ha atendido al cliente para la corrutina y la inicia de nuevo
            {
                Debug.Log("Se ha atendido al cliente");
                StopAllCoroutines();
                StartCoroutine(InicioCuentaAtras());
            }

            yield return null;
        }
        em.cantidadCom[pedido]--;
        CM.ClienteAdios(this.gameObject);
    }
}
