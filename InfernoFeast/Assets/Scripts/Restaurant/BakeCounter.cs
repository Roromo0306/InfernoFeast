using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BakeCounter : MonoBehaviour
{
    [Header("Padres")]
    public GameObject PadrePlayer;
    public GameObject PadreHorno;

    private int Indice;
    private bool ObjetoEncontrado = false; //Con este bool detectare si se ha encontrado un nombre en el if

    [Header("Listas")]
    public List<TipoIngrediente> horneados;
    public List<TipoIngrediente> ingredientes;

    [Header("UI")]
    public Slider slider;
    public float duracion = 7f;

    private Coroutine corrutina = null;

    //Funcion de hornear
    public void Hornear()
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

        GameObject objetoHorno = Instantiate(HijoPadre, PadreHorno.transform.position, PadreHorno.transform.rotation, PadreHorno.transform); //Instancio el objeto en el horno
        objetoHorno.name = HijoPadre.name; //Me aseguro que el nombre del nuevo objeto instanciado sea el correcto
        Destroy(HijoPadre);


        corrutina = StartCoroutine(ProcesoHornear(objetoHorno));

        Instanciar(objetoHorno);
    }


    private void Instanciar(GameObject HijoPadre)
    {
        //Si el bool es true pasa lo siguiente
        if (ObjetoEncontrado)
        {
            Destroy(HijoPadre); //Destruyo el objeto que llevaba el jugador

            GameObject nuevoObjeto = Instantiate(horneados[Indice].prefabIngrediente, PadrePlayer.transform.position, PadrePlayer.transform.rotation, PadrePlayer.transform); //Instancio el objeto equivalente en la lista de horneados
            nuevoObjeto.name = horneados[Indice].prefabIngrediente.name; //Me aseguro que el nombre del nuevo objeto instanciado sea el correcto

            Indice = 0;
        }
        else
        {
            GameObject nuevoObjeto = Instantiate(HijoPadre, PadrePlayer.transform.position, PadrePlayer.transform.rotation, PadrePlayer.transform); //Instancio el mismo objeto que llevaba el jugador
            nuevoObjeto.name = HijoPadre.name; //Me aseguro que el nombre sea el correcto

            Destroy(HijoPadre); //Destruyo el objeto que llevaba el jugador
        }
    }

    private IEnumerator ProcesoHornear(GameObject objetoHorno)
    {
        Debug.Log("hjh");
        //Preparamos el slider
        slider.gameObject.SetActive(true);
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;

        float tiempoPasado = 0f;
        while(tiempoPasado < duracion)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                //Se cancela
                //Instanciar(objetoHorno);
            }

            tiempoPasado += Time.deltaTime;
            slider.value = Mathf.Clamp01(tiempoPasado/duracion); //Fija el valor

            yield return null;
        }

        slider.gameObject.SetActive(false);
        //Completa el bake
        yield break;
    }
}
