using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GirarCamara : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return; //En caso de que no haya camara se detiene aqui la funcion

        transform.forward = Camera.main.transform.forward; //Hace que mire a la camara
    }
}
