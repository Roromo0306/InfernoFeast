using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MixCounter : MonoBehaviour
{
    [Header("Slider")]
    public Slider progressBar; //Barra de progreso
    public float progressIncrement = 1f; //Lo que aumenta por accion
    public float maxProgress = 100f; //Progreso maximo

    [Header("Sensibilidad de Movimiento")]
    public float circleThreshold = 0.5f; //Que tan grande debe ser el circulo
    public float rotationThreshold = 10.8f; //Cuantos grados de rotacion se necesitan para contar el progrso

    [Header("Padres")]
    public GameObject PadrePlayer;
    public GameObject PadreMix;

    private float currentProgress = 0f;
    private bool isInteracting = false;
    private Vector2 lastMouseDir;
    private float accumulatedRotation = 0f;

    private GameObject HijoMix;

    private int Indice;
    private bool ObjetoEncontrado = false; //Con este bool detectare si se ha encontrado un nombre en el if

    [Header("Listas")]
    public List<TipoIngrediente> batidos;
    public List<TipoIngrediente> ingredientes;

    void Update()
    {
        if (!isInteracting) return;

        if (Input.GetMouseButton(0)) // Solo mientras se mantiene presionado el botón izquierdo
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            if (mouseDelta.magnitude > circleThreshold)
            {
                Vector2 currentDir = mouseDelta.normalized;

                if (lastMouseDir != Vector2.zero)
                {
                    float angle = Vector2.SignedAngle(lastMouseDir, currentDir);
                    accumulatedRotation += Mathf.Abs(angle);

                    if (accumulatedRotation >= rotationThreshold)
                    {
                        AddProgress();
                        accumulatedRotation = 0f;
                    }
                }

                lastMouseDir = currentDir;
            }
        }
        else
        {
            lastMouseDir = Vector2.zero;
            accumulatedRotation = 0f;
        }
    }

    private void AddProgress()
    {
        currentProgress += progressIncrement;
        progressBar.value = currentProgress / maxProgress;

        if (currentProgress >= maxProgress)
        {
            EndMixing();
        }
    }

    public void StartMixing()
    {
        GameObject HijoPadre = PadrePlayer.transform.GetChild(0).gameObject; //Guardamos el gameobject que carga el player en un gameobject nuevo

        HijoMix = Instantiate(HijoPadre, PadreMix.transform.position, HijoPadre.transform.rotation, PadreMix.transform);
        HijoMix.name = HijoPadre.name;

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

        Destroy(HijoPadre); //Destruyo el objeto que llevaba el jugador
        progressBar.value = 0f;
        currentProgress = 0f;
        isInteracting = true;
        progressBar.gameObject.SetActive(true);
    }

    private void EndMixing()
    {
        //Si el bool es true pasa lo siguiente
        if (ObjetoEncontrado)
        {
            GameObject nuevoObjeto = Instantiate(batidos[Indice].prefabIngrediente, PadrePlayer.transform.position, batidos[Indice].prefabIngrediente.transform.rotation, PadrePlayer.transform); //Instancio el objeto equivalente en la lista de batidos
            nuevoObjeto.name = batidos[Indice].prefabIngrediente.name; //Me aseguro que el nombre del nuevo objeto instanciado sea el correcto

            Destroy(HijoMix);
            HijoMix = null;

            Indice = 0;
            ObjetoEncontrado = false;
        }
        else
        {
            /*GameObject nuevoObjeto = Instantiate(HijoPadre, PadrePlayer.transform.position, HijoPadre.transform.rotation, PadrePlayer.transform); //Instancio el mismo objeto que llevaba el jugador
            nuevoObjeto.name = HijoPadre.name; //Me aseguro que el nombre sea el correcto

            Destroy(HijoPadre); //Destruyo el objeto que llevaba el jugador*/
        }

        isInteracting = false;
        progressBar.gameObject.SetActive(false);
    }

   /* public void Batir()
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

            GameObject nuevoObjeto = Instantiate(batidos[Indice].prefabIngrediente, PadrePlayer.transform.position, batidos[Indice].prefabIngrediente.transform.rotation, PadrePlayer.transform); //Instancio el objeto equivalente en la lista de batidos
            nuevoObjeto.name = batidos[Indice].prefabIngrediente.name; //Me aseguro que el nombre del nuevo objeto instanciado sea el correcto

            Indice = 0;
            ObjetoEncontrado = false;
        }
        else
        {
            GameObject nuevoObjeto = Instantiate(HijoPadre, PadrePlayer.transform.position, HijoPadre.transform.rotation, PadrePlayer.transform); //Instancio el mismo objeto que llevaba el jugador
            nuevoObjeto.name = HijoPadre.name; //Me aseguro que el nombre sea el correcto

            Destroy(HijoPadre); //Destruyo el objeto que llevaba el jugador
        }
    }*/
}
