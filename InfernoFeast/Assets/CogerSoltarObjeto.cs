using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CogerSoltarObjeto : MonoBehaviour
{
    public GameObject Padre; //Lo usaremos para verificar si tiene hijos y por tanto si el jugador tiene un objeto

    [Header("Sitios donde se puede dejar objetos")]
    public GameObject Encimera1;
    public GameObject Encimera2;

    [Header("Gameobject padres")]
    public GameObject EncimeraPadre1;
    public GameObject EncimeraPadre2;

    public bool Hold;

    private void Update()
    {
        Hold = Padre.transform.childCount > 0; //Hold sera true si Padre tiene hijos
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject == Encimera1 && Hold)
        {
            GameObject objeto = Padre.transform.GetChild(0).gameObject;

            Destroy(Padre.transform.GetChild(0).gameObject);

            Instantiate(objeto, EncimeraPadre1.transform.position, EncimeraPadre1.transform.rotation, EncimeraPadre1.transform);

            objeto = null;
        }

        if(collision.gameObject == Encimera2 && Hold)
        {

        }
    }
}
