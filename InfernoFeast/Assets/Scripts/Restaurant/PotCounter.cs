using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PotCounter : MonoBehaviour
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
    public List<TipoIngrediente> hervidos;
    public List<TipoIngrediente> ingredientes;

    public void Hervir()
    {
        ObjetoEncontrado = false;
        GameObject HijoPadre = PadrePlayer.transform.GetChild(0).gameObject;
        string nombreHijo = HijoPadre.name.Replace("(Clone)", "").Trim(); //Esto elimina la palabra Clone y posibles espacios extras

        for (int i = 0; i < ingredientes.Count; i++)
        {
            if (ingredientes[i].name == nombreHijo)
            {
                Indice = i;
                ObjetoEncontrado = true;
                break;
            }
        }

        if (ObjetoEncontrado)
        {
            Destroy(HijoPadre);

            Instantiate(hervidos[Indice].prefabIngrediente, PadrePlayer.transform.position, PadrePlayer.transform.rotation, PadrePlayer.transform);
        }
        else
        {
            Instantiate(HijoPadre, PadrePlayer.transform.position, PadrePlayer.transform.rotation, PadrePlayer.transform);

            Destroy(HijoPadre);
        }
    }
}
