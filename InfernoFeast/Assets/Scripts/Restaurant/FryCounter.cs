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

    public void Freir()
    {
        GameObject HijoPadre = PadrePlayer.transform.GetChild(0).gameObject; //Guardamos el gameobject que carga el player en un gameobject nuevo

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

    private void InstanciarQuemado(GameObject HijoPadre)
    {
        Destroy(HijoPadre); //Destruyo el objeto que llevaba el jugador

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

        float tiempoPasado = 0f;
        while (tiempoPasado < duracion)
        {
            if (Input.GetKeyDown(KeyCode.R) && counterInt.Freir)
            {
                //Se cancela

                if (slider.value >= 0 && slider.value <= 0.7f)
                {
                    slider.gameObject.SetActive(false);
                    slider.value = 0f;
                    Instanciar(objetoFreidora);
                }

                if (slider.value > 0.7f)
                {
                    slider.gameObject.SetActive(false);
                    slider.value = 0f;
                    InstanciarQuemado(objetoFreidora);
                }


            }

            tiempoPasado += Time.deltaTime;
            slider.value = Mathf.Clamp01(tiempoPasado / duracion); //Fija el valor

            yield return null;
        }

        slider.gameObject.SetActive(false);
        //Completa el bake
        yield break;
    }
}
