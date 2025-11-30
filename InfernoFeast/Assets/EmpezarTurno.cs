using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EmpezarTurno : MonoBehaviour
{
    [Header("Texto Cuenta Atras")]
    public TextMeshProUGUI Cuenta; //Referencia al texto de la cuenta atras

    [Header("Player")]
    public GameObject Player;

    [Header("Lista Comandas")]
    public List<string> NombresComandas;
    public List<int> cantidadCom;

    [Header("Componentes UI Comandas")]
    public TextMeshProUGUI comanda1;
    public TextMeshProUGUI comanda2;
    public TextMeshProUGUI comanda3;
    public TextMeshProUGUI cant1;
    public TextMeshProUGUI cant2;
    public TextMeshProUGUI cant3;

   [HideInInspector] public bool empezado = false;

    private void Update()
    {
        //Textos de la primera comanda
        if (cantidadCom[0] > 0)
        {
            comanda1.gameObject.SetActive(true);
            cant1.gameObject.SetActive(true);

            cant1.text = "X"+cantidadCom[0];
        }
        else
        {
            comanda1.gameObject.SetActive(false);
            cant1.gameObject.SetActive(false);
        }

        //Textos de la segunda comanda
        if (cantidadCom[1] > 0)
        {
            comanda2.gameObject.SetActive(true);
            cant2.gameObject.SetActive(true);

            cant2.text = "X" + cantidadCom[1];
        }
        else
        {
            comanda2.gameObject.SetActive(false);
            cant2.gameObject.SetActive(false);
        }

        //Textos de la tercera comanda
        if (cantidadCom[2] > 0)
        {
            comanda3.gameObject.SetActive(true);
            cant3.gameObject.SetActive(true);

            cant3.text = "X" + cantidadCom[2];
        }
        else
        {
            comanda3.gameObject.SetActive(false);
            cant3.gameObject.SetActive(false);
        }
    }
    public void TurnoStart()
    {
        //Empezar cuenta atrás
        CuentaAtras();

        //Aparecer comandas
        empezado = true;
    }

    private void CuentaAtras()
    {
        StartCoroutine(CuentAtras());
    }

    IEnumerator CuentAtras()
    {
        InteractuarCounter inte = Player.GetComponent<InteractuarCounter>();

        float tiempo = 180f;
        while (tiempo > 0)
        {
            int minutos = (int)(tiempo / 60f);
            int segundos = (int)(tiempo % 60f);

            Cuenta.text = $"{minutos:00}:{segundos:00}";
            tiempo -= Time.deltaTime;

            yield return null;
        }

        Cuenta.text = "00:00";
        inte.turnoEmpezado = false;
        empezado = false;
    }
}
