using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class InteractuarCounter : MonoBehaviour
{
    public GameObject Padre;

    public bool Hold, Cortar, Pelar, Hornear, Hervir, Hervir2, Freir, Freir2, Batir, basura, Empezarturno, ObjetoDejado;

    [HideInInspector] public bool turnoEmpezado = false;

    public GameObject Counter, PadreFreir, PadreHorno, PadreHervir;
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

                if (!ObjetoDejado)
                {
                    bake.Hornear();
                }
            }

            if(Hervir && Input.GetKeyDown(KeyCode.E))
            {
                PotCounter pot = Counter.GetComponent<PotCounter>();
               
                if (!ObjetoDejado)
                {
                    pot.Hervir();
                }
            }

            if (Hervir2 && Input.GetKeyDown(KeyCode.E))
            {
                PotCounter pot = Counter.GetComponent<PotCounter>();

                if (!ObjetoDejado)
                {
                    pot.Hervir();
                }
            }

            if (Freir && Input.GetKeyDown(KeyCode.E))
            {
                FryCounter fry = Counter.GetComponent<FryCounter>();

                if (!ObjetoDejado)
                {
                    fry.Freir();
                }
            }

            if (Freir2 && Input.GetKeyDown(KeyCode.E))
            {
                FryCounter fry = Counter.GetComponent<FryCounter>();

                if (!ObjetoDejado)
                {
                    fry.Freir();
                }

            }

            if (Batir && Input.GetKeyDown(KeyCode.E))
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

        if (Empezarturno && Input.GetKeyDown(KeyCode.E))
        {
            if (!turnoEmpezado)
            {
                EmpezarTurno em = Counter.GetComponent<EmpezarTurno>();
                em.TurnoStart();
                turnoEmpezado = true;
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

            PadreHorno = Counter.transform.GetChild(0).gameObject;
            if(PadreHorno.transform.childCount > 0)
            {
                ObjetoDejado = true;
            }
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

            PadreFreir = Counter.transform.GetChild(0).gameObject;
            if (PadreFreir.transform.childCount > 0)
            {
                ObjetoDejado = true;
            }
        }

        if (collision.gameObject.name == "Freir2")
        {
            Freir2 = true;
            Counter = collision.gameObject;

            PadreFreir = Counter.transform.GetChild(0).gameObject;
            if (PadreFreir.transform.childCount > 0)
            {
                ObjetoDejado = true;
            }
        }

        if (collision.gameObject.name == "Hervir")
        {
            Hervir = true;
            Counter = collision.gameObject;

            PadreHervir = Counter.transform.GetChild(0).gameObject;
            if (PadreHervir.transform.childCount > 0)
            {
                ObjetoDejado = true;
            }
        }

        if (collision.gameObject.name == "Hervir2")
        {
            Hervir2 = true;
            Counter = collision.gameObject;

            PadreHervir = Counter.transform.GetChild(0).gameObject;
            if (PadreHervir.transform.childCount > 0)
            {
                ObjetoDejado = true;
            }
        }

        if (collision.gameObject.name == "Basura")
        {
            basura = true;
            Counter = collision.gameObject;
        }

        if (collision.gameObject.name == "EmpezarTurno")
        {
            Empezarturno = true;
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

            ObjetoDejado = false;
            PadreHorno = null;
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

            ObjetoDejado = false;
            PadreFreir = null;
        }

        if (collision.gameObject.name == "Freir2")
        {
            Freir2 = false;
            Counter = null;

            ObjetoDejado = false;
            PadreFreir = null;
        }

        if (collision.gameObject.name == "Hervir")
        {
            Hervir = false;
            Counter = null;

            ObjetoDejado = false;
            PadreHervir = null;
        }

        if (collision.gameObject.name == "Hervir2")
        {
            Hervir2 = false;
            Counter = null;

            ObjetoDejado = false;
            PadreHervir = null;
        }

        if (collision.gameObject.name == "Basura")
        {
            basura = false;
            Counter = null;
        }

        if (collision.gameObject.name == "EmpezarTurno")
        {
            Empezarturno = false;
            Counter = null;
        }
    }
}


