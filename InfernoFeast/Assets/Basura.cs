using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basura : MonoBehaviour
{
    public GameObject PadreJugador;

    public void Eliminar()
    {
        Destroy(PadreJugador.transform.GetChild(0).gameObject);
    }
}
