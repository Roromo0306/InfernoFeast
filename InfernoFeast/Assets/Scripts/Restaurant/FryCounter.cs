using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class FryCounter : MonoBehaviour
{
    [Header("Padres")]
    public GameObject PadrePlayer;
    public GameObject PadreFreidora;

    private int Indice;
    private bool ObjetoEncontrado = false; //Con este bool detectare si se ha encontrado un nombre en el if

    [Header("Listas")]
    public List<TipoIngrediente> fritos;
    public List<TipoIngrediente> ingredientes;

    [Header("UI")]
    public Slider slider;
    public float duracion = 7f;

    private Coroutine corrutina = null;

    public InteractuarCounter counterInt;
    public TipoIngrediente Quemado;

    public Image QuemadoImage;

    private bool quemado = false;

    public void Freir()
    {
        GameObject HijoPadre = PadrePlayer.transform.GetChild(0).gameObject; //Guardamos el gameobject que carga el player en un gameobject nuevo

        //Detecto el componente Estado Alimento para saber si dejar que el elemento se pueda freir
         EstadoAlimento Est = HijoPadre.GetComponent<EstadoAlimento>();
        if(Est.estado == 2 ||  Est.estado == 6 || Est.estado == 7)
        {
            return;
        }

        //Con este for recorre la lista entera hasta que encuentra un objeto que se llama igual que el objeto que lleva el jugador. Al encontrar esto, activo el bool y guardo el indice
        for (int i = 0; i < ingredientes.Count; i++)
        {
            if (ingredientes[i].name == HijoPadre.name)
            {
                Indice = i;
                ObjetoEncontrado = true;
                break;
            }
        }

        GameObject objetoFreidora = Instantiate(HijoPadre, PadreFreidora.transform.position, HijoPadre.transform.rotation, PadreFreidora.transform);
        objetoFreidora.name = HijoPadre.name;
        Destroy(HijoPadre);

        corrutina = StartCoroutine(ProcesoFreir(objetoFreidora));

    }

    private void Instanciar(GameObject HijoPadre)
    {
        //Si el bool es true pasa lo siguiente
        if (ObjetoEncontrado)
        {
            Destroy(HijoPadre); //Destruyo el objeto que llevaba el jugador

            GameObject nuevoObjeto = Instantiate(fritos[Indice].prefabIngrediente, PadrePlayer.transform.position, fritos[Indice].prefabIngrediente.transform.rotation, PadrePlayer.transform); //Instancio el objeto equivalente en la lista de fritos
            nuevoObjeto.name = fritos[Indice].prefabIngrediente.name; //Me aseguro que el nombre del nuevo objeto instanciado sea el correcto

            Indice = 0;
            ObjetoEncontrado = false;
        }
        else
        {
            GameObject nuevoObjeto = Instantiate(HijoPadre, PadrePlayer.transform.position, HijoPadre.transform.rotation, PadrePlayer.transform); //Instancio el mismo objeto que llevaba el jugador
            nuevoObjeto.name = HijoPadre.name; //Me aseguro que el nombre sea el correcto

            Destroy(HijoPadre); //Destruyo el objeto que llevaba el jugador
        }
    }

    private void InstanciarQuemado()
    {
        GameObject PadrePot = this.gameObject.transform.GetChild(0).gameObject;
        Destroy(PadrePot.gameObject.transform.GetChild(0).gameObject); //Destruyo el objeto que llevaba el jugador

        GameObject nuevoObjeto = Instantiate(Quemado.prefabIngrediente, PadrePlayer.transform.position, Quemado.prefabIngrediente.transform.rotation, PadrePlayer.transform); //Instancio el objeto equivalente en la lista de horneados
        nuevoObjeto.name = Quemado.prefabIngrediente.name; //Me aseguro que el nombre del nuevo objeto instanciado sea el correcto

        Indice = 0;
    }

    private IEnumerator ProcesoFreir(GameObject objetoFreidora)
    {
        //Preparamos el slider
        slider.gameObject.SetActive(true);
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;

        yield return null;
        float tiempoPasado = 0f;
        while (tiempoPasado < duracion)
        {
            tiempoPasado += Time.deltaTime;
            slider.value = Mathf.Clamp01(tiempoPasado / duracion); //Fija el valor


            if(this.gameObject.name == "Freir")
            {
                if (Input.GetKeyDown(KeyCode.E) && counterInt.Freir)
                {
                    if (PadrePlayer.transform.childCount <= 0)
                    {
                        //Se cancela

                        if (slider.value >= 0 && slider.value <= 0.9f)
                        {
                            slider.gameObject.SetActive(false);
                            slider.value = 0f;
                            Instanciar(objetoFreidora);
                            StopAllCoroutines();
                        }

                        if (slider.value >= 0.99f)
                        {
                            quemado = true;
                            QuemadoImage.enabled = true;
                            yield break;
                        }
                    }
                }
            }

            if (this.gameObject.name == "Freir2")
            {
                if (Input.GetKeyDown(KeyCode.E) && counterInt.Freir2)
                {
                    if (PadrePlayer.transform.childCount <= 0)
                    {
                        //Se cancela

                        if (slider.value >= 0 && slider.value <= 0.9f)
                        {
                            slider.gameObject.SetActive(false);
                            slider.value = 0f;
                            Instanciar(objetoFreidora);
                            StopAllCoroutines();
                        }

                        if (slider.value >= 0.99f)
                        {
                            quemado = true;
                            QuemadoImage.enabled = true;
                            yield break;
                        }
                    }
                }
            }

            yield return null;
        }

        //Completa el bake
        quemado = true;
        QuemadoImage.enabled = true;
        StopAllCoroutines();
        yield break;
    }

    private void Update()
    {
        if (quemado && Input.GetKeyDown(KeyCode.E) && counterInt.Freir)
        {
            slider.gameObject.SetActive(false);
            slider.value = 0f;
            QuemadoImage.enabled = false;
            InstanciarQuemado();
            quemado = false;
        }
    }
}
