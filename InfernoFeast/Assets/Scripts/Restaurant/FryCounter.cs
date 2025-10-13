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
    private bool ObjetoEncontrado; //Con este bool detectare si se ha encontrado un nombre en el if

    [Header("Listas")]
    public List<TipoIngrediente> fritos;
    public List<TipoIngrediente> ingredientes;

    public void Freir()
    {
        ObjetoEncontrado = false;
        GameObject HijoPadre = PadrePlayer.transform.GetChild(0).gameObject;

        for (int i = 0; i < ingredientes.Count; i++)
        {
            if (ingredientes[i].name == HijoPadre.name)
            {
                Indice = i;
                ObjetoEncontrado = true;
                break;
            }
        }

        if (ObjetoEncontrado)
        {
            Destroy(HijoPadre);

            GameObject nuevoObjeto = Instantiate(fritos[Indice].prefabIngrediente, PadrePlayer.transform.position, PadrePlayer.transform.rotation, PadrePlayer.transform);
            nuevoObjeto.name = fritos[Indice].prefabIngrediente.name;
        }
        else
        {
            GameObject nuevoObjeto = Instantiate(HijoPadre, PadrePlayer.transform.position, PadrePlayer.transform.rotation, PadrePlayer.transform);
            nuevoObjeto.name = HijoPadre.name;

            Destroy(HijoPadre);
        }
    }
}
