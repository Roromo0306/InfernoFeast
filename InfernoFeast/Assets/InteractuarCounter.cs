using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractuarCounter : MonoBehaviour
{
    public GameObject Padre;

    public bool Hold, Cortar, Pelar, Hornear, Hervir, Freir, Batir, basura;

    private GameObject Counter;
    private void Update()
    {
        Hold = Padre.transform.childCount > 0;

        if (Hold)
        {
            if (Cortar && Input.GetKeyDown(KeyCode.E))
            {
                CutCounter cut = Counter.GetComponent<CutCounter>();
                cut.cortar();
            }

            if(Hornear && Input.GetKeyDown(KeyCode.E))
            {
                BakeCounter bake = Counter.GetComponent<BakeCounter>();
                bake.Hornear();
            }

            if(Hervir && Input.GetKeyDown(KeyCode.E))
            {
                PotCounter pot = Counter.GetComponent<PotCounter>();
                pot.Hervir();
            }

            if(Freir && Input.GetKeyDown(KeyCode.E))
            {
                FryCounter fry = Counter.GetComponent<FryCounter>();
                fry.Freir();
            }

            if(Batir && Input.GetKeyDown(KeyCode.E))
            {
                MixCounter mix = Counter.GetComponent<MixCounter>();
                mix.StartMixing();
            }

            if(basura && Input.GetKeyDown(KeyCode.E))
            {
                Basura bas = Counter.GetComponent<Basura>();
                bas.Eliminar();
            }
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Cortar")
        {
            Cortar = true;
            Counter = collision.gameObject;
        }

        if (collision.gameObject.name == "Pelar")
        {
            Pelar = true;
            Counter = collision.gameObject;
        }

        if (collision.gameObject.name == "Horno")
        {
            Hornear = true;
            Counter = collision.gameObject;
        }

        if (collision.gameObject.name == "Batir")
        {
            Batir = true;
            Counter = collision.gameObject;
        }

        if (collision.gameObject.name == "Freir")
        {
            Freir = true;
            Counter = collision.gameObject;
        }

        if(collision.gameObject.name == "Hervir")
        {
            Hervir = true;
            Counter = collision.gameObject;
        }

        if(collision.gameObject.name == "Basura")
        {
            basura = true;
            Counter = collision.gameObject;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.name == "Cortar")
        {
            Cortar = false;
            Counter = null;
        }

        if (collision.gameObject.name == "Pelar")
        {
            Pelar = false;
            Counter = null;
        }

        if (collision.gameObject.name == "Horno")
        {
            Hornear = false;
            Counter = null;
        }

        if (collision.gameObject.name == "Batir")
        {
            Batir = false;
            Counter = null;
        }

        if (collision.gameObject.name == "Freir")
        {
            Freir = false;
            Counter = null;
        }

        if (collision.gameObject.name == "Hervir")
        {
            Hervir = false;
            Counter = null;
        }

        if(collision.gameObject.name == "Basura")
        {
            basura = false;
            Counter = null;
        }
    }
}


