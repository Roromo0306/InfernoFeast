using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractuarCounter : MonoBehaviour
{
    public GameObject Padre;

    [HideInInspector] public bool Hold, Cortar, Pelar, Hornear, Hornear2, Hervir, Hervir2, Freir, Freir2, Batir, basura, Empezarturno, ObjetoDejado;

    [HideInInspector] public bool turnoEmpezado = false;

    [HideInInspector] public GameObject Counter, PadreFreir, PadreHorno, PadreHervir;

    [Header("Imagenes E")]
    public GameObject CortarE, Cortar2E, HornearE, Hornear2E, HervirE, Hervir2E, FreirE, Freir2E, BatirE, Batir2E, BasuraE, EmpezarTurnoE, CajaCarneE, CajaPescadoE, CajaEspeciasE, CajaPanE, CajaVegetalesE, EncimeraE, Encimera2E;
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

            if (Hornear2 && Input.GetKeyDown(KeyCode.E))
            {
                BakeCounter bake = Counter.GetComponent<BakeCounter>();

                if (!ObjetoDejado)
                {
                    bake.Hornear();
                }
            }

            if (Hervir && Input.GetKeyDown(KeyCode.E))
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

    //Entrada de la colision

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Cortar" || collision.gameObject.name == "Cortar2")
        {
            Cortar = true;
            Counter = collision.gameObject;
            Debug.Log("Hola");

            if(collision.gameObject.name == "Cortar")
            {
                CortarE.gameObject.SetActive(true);
                Debug.Log("Hola2");
            }
            else
            {
                Cortar2E.gameObject.SetActive(true);
            }
            
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
            HornearE.gameObject.SetActive(true);

            PadreHorno = Counter.transform.GetChild(0).gameObject;
            if(PadreHorno.transform.childCount > 0)
            {
                ObjetoDejado = true;
            }
        }

        if (collision.gameObject.name == "Horno2")
        {
            Hornear2 = true;
            Counter = collision.gameObject;
            Hornear2E.gameObject.SetActive(true);

            PadreHorno = Counter.transform.GetChild(0).gameObject;
            if (PadreHorno.transform.childCount > 0)
            {
                ObjetoDejado = true;
            }
        }

        if (collision.gameObject.name == "Batir" || collision.gameObject.name == "Batir2")
        {
            Batir = true;
            Counter = collision.gameObject;

            if (collision.gameObject.name == "Batir")
            {
                BatirE.gameObject.SetActive(true);
            }
            else
            {
                Batir2E.gameObject.SetActive(true);
            }
        }

        if (collision.gameObject.name == "Freir")
        {
            Freir = true;
            Counter = collision.gameObject;
            FreirE.gameObject.SetActive(true);

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
            Freir2E.gameObject.SetActive(true);

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
            HervirE.gameObject.SetActive(true);

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
            Hervir2E.gameObject.SetActive(true);

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
            BasuraE.gameObject.SetActive(true);
        }

        if (collision.gameObject.name == "EmpezarTurno")
        {
            Empezarturno = true;
            Counter = collision.gameObject;
            EmpezarTurnoE.gameObject.SetActive(true);
        }

        if(collision.gameObject.name == "CajaCarne")
        {
            CajaCarneE.gameObject.SetActive(true);
        }

        if (collision.gameObject.name == "CajaPescado")
        {
            CajaPescadoE.gameObject.SetActive(true);
        }

        if (collision.gameObject.name == "CajaEspecias")
        {
            CajaEspeciasE.gameObject.SetActive(true);
        }

        if (collision.gameObject.name == "CajaPan")
        {
            CajaPanE.gameObject.SetActive(true);
        }

        if (collision.gameObject.name == "CajaVerdura")
        {
            CajaVegetalesE.gameObject.SetActive(true);
        }

        if (collision.gameObject.name == "Encimera")
        {
            EncimeraE.gameObject.SetActive(true);
        }

        if (collision.gameObject.name == "Encimera2")
        {
            Encimera2E.gameObject.SetActive(true);
        }
    }

    //Salida de la colision
    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.name == "Cortar" || collision.gameObject.name == "Cortar2")
        {
            Cortar = false;
            Counter = null;
            if (collision.gameObject.name == "Cortar")
            {
                CortarE.gameObject.SetActive(false);
            }
            else
            {
                Cortar2E.gameObject.SetActive(false);
            }
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
            HornearE.gameObject.SetActive(false);

            ObjetoDejado = false;
            PadreHorno = null;
        }

        if (collision.gameObject.name == "Horno2")
        {
            Hornear2 = false;
            Counter = null;
            Hornear2E.gameObject.SetActive(false);

            ObjetoDejado = false;
            PadreHorno = null;
        }

        if (collision.gameObject.name == "Batir" || collision.gameObject.name == "Batir2")
        {
            Batir = false;
            Counter = null;

            if (collision.gameObject.name == "Batir")
            {
                BatirE.gameObject.SetActive(false);
            }
            else
            {
                Batir2E.gameObject.SetActive(false);
            }
        }

        if (collision.gameObject.name == "Freir")
        {
            Freir = false;
            Counter = null;
            FreirE.gameObject.SetActive(false);

            ObjetoDejado = false;
            PadreFreir = null;
        }

        if (collision.gameObject.name == "Freir2")
        {
            Freir2 = false;
            Counter = null;
            Freir2E.gameObject.SetActive(false);

            ObjetoDejado = false;
            PadreFreir = null;
        }

        if (collision.gameObject.name == "Hervir")
        {
            Hervir = false;
            Counter = null;
            HervirE.gameObject.SetActive(false);

            ObjetoDejado = false;
            PadreHervir = null;
        }

        if (collision.gameObject.name == "Hervir2")
        {
            Hervir2 = false;
            Counter = null;
            Hervir2E.gameObject.SetActive(false);

            ObjetoDejado = false;
            PadreHervir = null;
        }

        if (collision.gameObject.name == "Basura")
        {
            basura = false;
            Counter = null;
            BasuraE.gameObject.SetActive(false);
        }

        if (collision.gameObject.name == "EmpezarTurno")
        {
            Empezarturno = false;
            Counter = null;
            EmpezarTurnoE.gameObject.SetActive(false);
        }

        if (collision.gameObject.name == "CajaCarne")
        {
            CajaCarneE.gameObject.SetActive(false);
        }

        if (collision.gameObject.name == "CajaPescado")
        {
            CajaPescadoE.gameObject.SetActive(false);
        }

        if (collision.gameObject.name == "CajaEspecias")
        {
            CajaEspeciasE.gameObject.SetActive(false);
        }

        if (collision.gameObject.name == "CajaPan")
        {
            CajaPanE.gameObject.SetActive(false);
        }

        if (collision.gameObject.name == "CajaVerdura")
        {
            CajaVegetalesE.gameObject.SetActive(false);
        }

        if (collision.gameObject.name == "Encimera")
        {
            EncimeraE.gameObject.SetActive(false);
        }

        if (collision.gameObject.name == "Encimera2")
        {
            Encimera2E.gameObject.SetActive(false);
        }
    }
}


