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

    public bool Elegido = false;

    private void Start()
    {
        if(EmpezarTurnoCounter == null)
        {
            EmpezarTurnoCounter = GameObject.Find("EmpezarTurno");
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


    }

    private void ElegirComandaV()
    {

    }
}
