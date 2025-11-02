using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CogerSoltarObjeto : MonoBehaviour
{
    public GameObject Padre; //Lo usaremos para verificar si tiene hijos y por tanto si el jugador tiene un objeto

    [Header("Sitios donde se puede dejar objetos")]
    public GameObject Encimera1;
    public GameObject Encimera2;

    public bool Hold, EncimeraSoltar, EncimeraCoger;

    private GameObject EncimeraCounter;

    private void Update()
    {
        Hold = Padre.transform.childCount > 0; //Hold sera true si Padre tiene hijos

        if(EncimeraSoltar && Input.GetKeyDown(KeyCode.E))
        {
            SoltarObjeto(EncimeraCounter);
        }

        if(EncimeraCoger && Input.GetKeyDown(KeyCode.E))
        {
            CogerObjeto(EncimeraCounter);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(Hold && collision.gameObject.CompareTag("Encimera"))
        {
            EncimeraSoltar = true;
            EncimeraCounter = collision.gameObject;
        }

        if(!Hold && collision.gameObject.CompareTag("Encimera"))
        {
            EncimeraCoger = true;
            EncimeraCounter = collision.gameObject;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!Hold && collision.gameObject.CompareTag("Encimera"))
        {
            EncimeraSoltar = false;
            EncimeraCoger = false;
            EncimeraCounter = null;
        }

        if (Hold && collision.gameObject.CompareTag("Encimera"))
        {
            EncimeraCoger = false;
            EncimeraSoltar = false;
            EncimeraCounter = null;
        }
    }

    //Funcion para soltar el objeto en una encimera
    private void SoltarObjeto(GameObject collision)
    {
        if (Padre.transform.childCount > 0)
        {
            GameObject objeto = Padre.transform.GetChild(0).gameObject;
            GameObject PadreEncimera = collision.transform.GetChild(0).gameObject;

            if (PadreEncimera.transform.childCount == 0)
            {
                GameObject newObj = Instantiate(objeto, PadreEncimera.transform.position, objeto.transform.rotation, PadreEncimera.transform);
                newObj.name = newObj.name.Replace("(Clone)", "").Trim();

                // asegurar que la fisica quede inactiva cuando está en la encimera (opcional)
                Rigidbody rbNew = newObj.GetComponent<Rigidbody>();
                if (rbNew != null)
                {
                    rbNew.isKinematic = true;
                    rbNew.useGravity = false;
                }

                Destroy(objeto);
                objeto = null;
                PadreEncimera = null;
            }
            else
            {
                Encimera enci = collision.GetComponent<Encimera>();

                enci.objeto2 = objeto.gameObject;

                Destroy(objeto);

                objeto = null;
                PadreEncimera = null;
            }
        }
    }

    private void CogerObjeto(GameObject collision)
    {
        GameObject PadreEncimera = collision.transform.GetChild(0).gameObject;

        if (PadreEncimera.transform.childCount > 0)
        {
            Debug.Log("Cogido");
            GameObject objeto = PadreEncimera.transform.GetChild(0).gameObject;

            GameObject newObj = Instantiate(objeto, Padre.transform.position, objeto.transform.rotation, Padre.transform);
            newObj.name = newObj.name.Replace("(Clone)", "").Trim();

            // IMPORTANT: dejar la rigidbody en kinematic mientras está en la mano
            Rigidbody rbNew = newObj.GetComponent<Rigidbody>();
            if (rbNew != null)
            {
                rbNew.isKinematic = true;
                rbNew.useGravity = false;
                rbNew.velocity = Vector3.zero;
                rbNew.angularVelocity = Vector3.zero;
            }

            // ajustar posición local en caso de que haga falta
            newObj.transform.localPosition = Vector3.zero;
            newObj.transform.localRotation = Quaternion.identity;

            Destroy(objeto);

            objeto = null;
            PadreEncimera = null;
        }
    }
}
