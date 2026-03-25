using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EmpezarTurno : MonoBehaviour
{
    [Header("Texto Cuenta Atras")]
    public TextMeshProUGUI Cuenta; //Referencia al texto de la cuenta atras

    [Header("Player")]
    public GameObject Player;

    [Header("Lista Comandas")]
    public List<Sprite> NombresComandas; //Estos son los nombres de las comandas que pueden aparecer
    public List<int> cantidadCom; //Esta es la cantidad de comandas de un mismo plato que se han pedido

    [Header("Componentes UI Comandas")]
    public GameObject comanda1; //Estos hacen referencia a la imagen de la comanda que va a aparecer
    public GameObject comanda2;
    public GameObject comanda3;
    public TextMeshProUGUI cant1; //Estos textos hacen referencia a la cantidad que se ha pedido de esa comanda
    public TextMeshProUGUI cant2;
    public TextMeshProUGUI cant3;

    [Header("Componentes UI Comandas")]
    public List<Sprite> ListaComandas;

    [Header("Referencia a la UI de abierto o cerrado")]
    public Sprite abierto;
    public Sprite cerrado;
    public Image AbCer;

   [HideInInspector] public bool empezado = false;

    [Header("Variables dinero y reputacion")]
    public int dineroTurno;
    public int reputacionTurno;

    [Header("Sonido")]
    public AudioSource audio;

    [Header("UI Lista Comandas (nuevo sistema)")]
    public Transform contenedorComandas;
    public GameObject prefabComandaUI;
    public int maxComandas = 4;
    public float separacionY = 90f;
    public float desplazamientoEntrada = 500f;

    private List<ComandaActiva> comandasActivas = new List<ComandaActiva>();

    private void Update()
    {
        //Textos de la primera comanda
        if (cantidadCom[0] > 0)
        {
            comanda1.gameObject.SetActive(true);
            cant1.gameObject.SetActive(true);

            cant1.text = "X"+cantidadCom[0];
        }
        else
        {
            comanda1.gameObject.SetActive(false);
            cant1.gameObject.SetActive(false);
        }

        //Textos de la segunda comanda
        if (cantidadCom[1] > 0)
        {
            comanda2.gameObject.SetActive(true);
            cant2.gameObject.SetActive(true);

            cant2.text = "X" + cantidadCom[1];
        }
        else
        {
            comanda2.gameObject.SetActive(false);
            cant2.gameObject.SetActive(false);
        }

        //Textos de la tercera comanda
        if (cantidadCom[2] > 0)
        {
            comanda3.gameObject.SetActive(true);
            cant3.gameObject.SetActive(true);

            cant3.text = "X" + cantidadCom[2];
        }
        else
        {
            comanda3.gameObject.SetActive(false);
            cant3.gameObject.SetActive(false);
        }
    }
    public void TurnoStart()
    {
        //Empezar cuenta atrás
        CuentaAtras();

        //Aparecer comandas
        empezado = true;

        //Cambiamos el cartel a abierto
        AbCer.sprite = abierto;

        //Iniciamos el sonido
        audio.Play();
    }

    private void CuentaAtras()
    {
        StartCoroutine(CuentAtras());
    }

    IEnumerator CuentAtras()
    {
        InteractuarCounter inte = Player.GetComponent<InteractuarCounter>();

        ElegirComandas();

        float tiempo = 180f;
        while (tiempo > 0)
        {
            int minutos = (int)(tiempo / 60f);
            int segundos = (int)(tiempo % 60f);

            Cuenta.text = $"{minutos:00}:{segundos:00}";
            tiempo -= Time.deltaTime;

            yield return null;
        }

        Cuenta.text = "00:00";
        inte.turnoEmpezado = false;
        empezado = false;
        AbCer.sprite = cerrado;
        FinDeDiaVariables();
    }

    private void ElegirComandas()
    {
        NombresComandas.Clear(); //Limpiamos la lista por si acaso

        List<Sprite> copia = new List<Sprite>(ComandasManager.Instance.NombresComandasTotales); //Creamos una copia para evitar que salgan tres comandas iguales

        for(int i = 0; i < 3 && copia.Count > 0; i++)
        {
            int r = Random.Range(0, copia.Count);
            NombresComandas.Add(copia[r]);
            copia.RemoveAt(r);
        }
    }

    private void FinDeDiaVariables()
    {
        //Annado la reputacion y el dinero al singleton
        ManagerFinDia.Instance.dinero += dineroTurno;
        ManagerFinDia.Instance.reputacion += reputacionTurno;

        //Reseteo el dinero
        dineroTurno = 0;
        reputacionTurno = 0;
    }

    public void ComandaUI(string nombre)
    {
        if (comandasActivas.Count >= maxComandas)
            return;

        Sprite sprite = null;

        for (int i = 0; i < ListaComandas.Count; i++)
        {
            if (ListaComandas[i].name == nombre)
            {
                sprite = ListaComandas[i];
                break;
            }
        }

        if (sprite == null)
        {
            Debug.LogWarning("No se encontró el sprite de la comanda: " + nombre);
            return;
        }

        GameObject nueva = Instantiate(prefabComandaUI, contenedorComandas);
        nueva.name = "Comanda_" + nombre + "_" + comandasActivas.Count;

        Image img = nueva.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("El prefabComandaUI no tiene componente Image.");
            Destroy(nueva);
            return;
        }

        img.sprite = sprite;

        RectTransform rt = nueva.GetComponent<RectTransform>();
        float targetY = -comandasActivas.Count * separacionY;

        rt.anchoredPosition = new Vector2(desplazamientoEntrada, targetY);

        comandasActivas.Add(new ComandaActiva
        {
            objeto = nueva,
            nombre = nombre
        });

        StartCoroutine(AnimarEntrada(rt, targetY));
    }

    /* IEnumerator Animacion()
     {
         Animator anim = prefabImagen.GetComponent<Animator>();

         anim.enabled = true;
         anim.Play("animacionComandas");

         yield return new WaitForSeconds(0.25f);

         FijarPosicionFinal();
         anim.enabled = false;

         yield break;
     }*/

    IEnumerator AnimarEntrada(RectTransform rt, float targetY)
    {
        Vector2 start = new Vector2(desplazamientoEntrada, targetY);
        Vector2 end = new Vector2(0f, targetY);

        float t = 0f;
        float duracion = 0.25f;

        while (t < duracion)
        {
            t += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(start, end, t / duracion);
            yield return null;
        }

        rt.anchoredPosition = end;
    }

    void ReordenarComandas()
    {
        for (int i = 0; i < comandasActivas.Count; i++)
        {
            if (comandasActivas[i].objeto == null) continue;

            RectTransform rt = comandasActivas[i].objeto.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, -i * separacionY);
        }
    }

    public void EliminarComanda(string nombre)
    {
        for (int i = 0; i < comandasActivas.Count; i++)
        {
            if (comandasActivas[i].nombre == nombre)
            {
                Destroy(comandasActivas[i].objeto);
                comandasActivas.RemoveAt(i);
                ReordenarComandas();
                return;
            }
        }
    }

    [System.Serializable]
    public class ComandaActiva
    {
        public GameObject objeto;
        public string nombre;
    }

    /*void FijarPosicionFinal()
    {
        RectTransform rt = prefabImagen.rectTransform;
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 419);
    }

    public void PosicionEntregarPlato()
    {
        RectTransform rt = prefabImagen.rectTransform;
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 752);
    }*/
}
