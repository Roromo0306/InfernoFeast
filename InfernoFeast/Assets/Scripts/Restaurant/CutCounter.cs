using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutCounter : MonoBehaviour
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

    [Header("Listas")]
    public List<GameObject> cortados;
    public List<GameObject> ingredientes;

    private void cortar()
    {
        for(int i = 0; i < ingredientes.Count; i++)
        {
            if(ingredientes[i].name == PadrePlayer.transform.GetChild(0).name)
            {
                Indice = i;
                return;
            }
        }
    }
}
