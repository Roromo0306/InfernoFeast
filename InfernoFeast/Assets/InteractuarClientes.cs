using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractuarClientes : MonoBehaviour
{
    public GameObject EmpezarTurnoCounter;

    public int ClienteTipo = 0; //Tipo 1 = normal. Tipo 2 = VIP;

    [Header("Canvas Propio")]
    public TextMeshProUGUI comanda;

    [Header("Cliente Manager")]
    public GameObject clienteManager;

    public bool Elegido = false;
    private bool Atendido = false; //Este bool controlara si se ha atendido al cliente.
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
            //Clientes normales
            if (collision.gameObject.CompareTag("Player") && ClienteTipo == 1 && !Elegido) 
            {
                Debug.Log("2");
                ElegirComandaN();
                Elegido = true;
            }

            //Clientes VIP
            if (collision.gameObject.CompareTag("Player") && ClienteTipo == 2 && !Elegido)
            {
                ElegirComandaV();
                Elegido = true;
            }
        }
    }

    private void ElegirComandaN()
    {
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        int pedido = Random.Range(0, 3);

        comanda.text = em.NombresComandas[pedido];
        em.cantidadCom[pedido]++;

        //Inicia cuenta atrás
        float tiempo = 60f;
        Atendido = true;

        //Si le da el plato llamar a ClienteAdios() de ClienteManger

        //Desaparecer tras acabarse la cuenta atras

    }

    private void ElegirComandaV()
    {
        EmpezarTurno em = EmpezarTurnoCounter.GetComponent<EmpezarTurno>();

        int pedido = Random.Range(0, 3);

        comanda.text = em.NombresComandas[pedido];
        em.cantidadCom[pedido]++;
    }

    IEnumerator InicioCuentaAtras()
    {
        ClienteManager CM = clienteManager.GetComponent<ClienteManager>(); //Referencia a cliente Manager

        float tiempo = 0f; //Creo la variable tiempo (por mi la hubiera creado directamente en el if pero daba error)

        if (!Atendido) //Si no se ha atendido al cliente el tiempo es 45
        {
            tiempo = 45f;
        }
        else //Si se ha atendido al cliente el tiempo es 60
        {
            tiempo = 60f;
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

        CM.ClienteAdios(this.gameObject);
    }
}
