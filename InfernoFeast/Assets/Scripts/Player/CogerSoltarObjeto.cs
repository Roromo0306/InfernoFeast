using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CogerSoltarObjeto : MonoBehaviour
{
    public GameObject Padre; //Lo usaremos para verificar si tiene hijos y por tanto si el jugador tiene un objeto

    [Header("Sitios donde se puede dejar objetos")]
    public GameObject Encimera1;
    public GameObject Encimera2;

    public bool Hold;

    private void Update()
    {
        Hold = Padre.transform.childCount > 0; //Hold sera true si Padre tiene hijos
    }

    private void OnCollisionStay(Collision collision)
    {
        if(Hold && collision.gameObject.CompareTag ("Encimera"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                SoltarObjeto(collision);
            }
        }

        if(!Hold && collision.gameObject.CompareTag("Encimera"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                CogerObjeto(collision);
            }
        }
    }

    //Funcion para soltar el objeto en una encimera
    private void SoltarObjeto(Collision collision)
    {
        if(Padre.transform.childCount > 0) //Aunque esto ya exista en hold, con esto evitamos errores que pueden aparecer entre frames.
        {
            GameObject objeto = Padre.transform.GetChild(0).gameObject; //Obtenemos y guardamos el objeto hijo en un gameobject nuevo
            GameObject PadreEncimera = collision.transform.GetChild(0).gameObject; //Obtenemos y guardamos el padre de la encimera, donde se instanciara el nuevo 

            Instantiate(objeto, PadreEncimera.transform.position, PadreEncimera.transform.rotation, PadreEncimera.transform); //Instanciamos en el nuevo lugar

            Destroy(objeto); //Destruimos el original

            objeto = null; //Reseteamos objeto
            PadreEncimera = null;
        }
    }

    private void CogerObjeto(Collision collision)
    {
        GameObject PadreEncimera = collision.transform.GetChild(0).gameObject; //Obtenemos y guardamos el lugar esta guardado el objeto en la encimera
        
        if(PadreEncimera.transform.childCount > 0) //Realizamos esto para evitar errores por ejecutarse entre frames y no encontrar hijos
        {
            GameObject objeto = PadreEncimera.transform.GetChild(0).gameObject; //Obtenemos y guardamos el objeto que esta en la encimera

            Instantiate(objeto, Padre.transform.position, Padre.transform.rotation, Padre.transform); //Instanciamos el objeto en el GameObject vacio del jugador

            Destroy(objeto); //Destruimos el objeto

            objeto = null; //Reseteamos el objeto en caso de que haya quedado algun dato de el
            PadreEncimera = null;
        }
        
    }
}
