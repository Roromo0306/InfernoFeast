using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FryCounter : MonoBehaviour
{
    [Header("Slider")]
    public Slider progressBar; //Barra de progreso
    public float progressIncrement = 0.1f; //Lo que aumenta por accion
    public float maxProgress = 100f; //Progreso maximo

    public GameObject PadrePlayer;

    private float currentProgress = 0f;
    private bool isInteracting = false;
    private int lastMouseButton = -1;
    private int Indice;
    private bool ObjetoEncontrado = false; //Con este bool detectare si se ha encontrado un nombre en el if

    [Header("Listas")]
    public List<TipoIngrediente> fritos;
    public List<TipoIngrediente> ingredientes;

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

        //Si el bool es true pasa lo siguiente
        if (ObjetoEncontrado)
        {
            Destroy(HijoPadre); //Destruyo el objeto que llevaba el jugador

            GameObject nuevoObjeto = Instantiate(fritos[Indice].prefabIngrediente, PadrePlayer.transform.position, PadrePlayer.transform.rotation, PadrePlayer.transform); //Instancio el objeto equivalente en la lista de fritos
            nuevoObjeto.name = fritos[Indice].prefabIngrediente.name; //Me aseguro que el nombre del nuevo objeto instanciado sea el correcto

            Indice = 0;
        }
        else
        {
            GameObject nuevoObjeto = Instantiate(HijoPadre, PadrePlayer.transform.position, PadrePlayer.transform.rotation, PadrePlayer.transform); //Instancio el mismo objeto que llevaba el jugador
            nuevoObjeto.name = HijoPadre.name; //Me aseguro que el nombre sea el correcto

            Destroy(HijoPadre); //Destruyo el objeto que llevaba el jugador
        }
    }
}
