using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Instrucciones : MonoBehaviour
{
    public GameObject instrucciones;
    void Start()
    {
        instrucciones.SetActive(false);
    }

    public void Abrir()
    {
        instrucciones.SetActive(true);
    }

    public void Cerrar()
    {
        instrucciones.SetActive(false);
    }
}
