using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dormir : MonoBehaviour
{
    public GameObject EndDayPannel;

    [HideInInspector] public bool EnContacto = false, nuevoDia = false;


    private void NuevoDia()
    {
        EndDayPannel.gameObject.SetActive(true);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == "Player")
        {
            EnContacto = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == "Player")
        {
            EnContacto = false;
        }
    }

    private void Update()
    {
        if(EnContacto && Input.GetKey(KeyCode.E) && !nuevoDia)
        {
            nuevoDia = true;
            NuevoDia();
        }
    }

}
