using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutCounter : MonoBehaviour
{
    [Header("Slider")] 
    public Slider slider; //Slider
    public KeyCode primeraTecla = KeyCode.Mouse0; //Boton izquierdo del raton
    public KeyCode segundaTecla = KeyCode.Mouse1; //Boton derecho del raton

    private bool interaccionAcriva = false; //Bool para comprobar si la mecanica se ha activado
    private bool IzquierdaClick = false; //Bool para comprobar si se ha dado click al boton izquierdo del raton

    [Header("Padres")]
    public GameObject PadrePlayer; //Padre del player
    public GameObject PadreCortar; //Padre del counter

    private int Indice; //Donde se guardara el indice de la lista para instanciar desde la otra lista
    private bool ObjetoEncontrado = false; //Con este bool detectare si se ha encontrado un nombre en el if
    private GameObject HijoCortar; //Donde se guarda el objeto hijo al dejarlo para pasarlo como referencia a la funcion de terminar.

    [Header("Listas")]
    public List<TipoIngrediente> cortados; //Lista de los objetos cortados
    public List<TipoIngrediente> ingredientes; //Lista de los ingredientes

    private void Awake()
    {
        slider.gameObject.SetActive(false); //Activo el slider
        slider.maxValue = 8; //Establezco en 8 el valor maximo del slider
    }
    private void Update()
    {
        if (!interaccionAcriva) return; //Revisa si la esta funionando la mecanica de cortar y si no lo esta para y no continua

        if (!IzquierdaClick) //Click izquierdo
        {
            if(Input.GetKeyDown(primeraTecla))
            {
                IzquierdaClick = true;
                slider.value += 1;
            }
        }
        else //Click derecho
        {
            if (Input.GetKeyDown(segundaTecla))
            {
                slider.value += 1;
                IzquierdaClick = false;
            }
        }

        if(slider.value >= slider.maxValue) //Pone fin a la iteracion cuando el valor del slider llega a 8
        {
            FinInteraccion(HijoCortar);
        }
    }

    public void cortar()
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

        HijoCortar = Instantiate(HijoPadre, PadreCortar.transform.position, HijoPadre.transform.rotation, PadreCortar.transform); //Intancia el objeto en el counter
        HijoCortar.name = HijoPadre.name; //Le da al objeto el nombre correcto
        Destroy(HijoPadre); //Destruye el objeto

        Empezar();

    }

    //Funcion para empezar la accion con el slider
    private void Empezar()
    {
        interaccionAcriva = true;
        IzquierdaClick = false;
        slider.value = slider.minValue;
        slider.gameObject.SetActive(true);
    }

    //Funcion para terminar la accion y que instancia el objeto cortado
    private void FinInteraccion(GameObject HijoPadre)
    {
        //Si el bool es true pasa lo siguiente
        if (ObjetoEncontrado)
        {
            Destroy(HijoPadre); //Destruyo el objeto que llevaba el jugador

            GameObject nuevoObjeto = Instantiate(cortados[Indice].prefabIngrediente, PadrePlayer.transform.position, cortados[Indice].prefabIngrediente.transform.rotation, PadrePlayer.transform); //Instancio el objeto equivalente en la lista de cortados
            nuevoObjeto.name = cortados[Indice].prefabIngrediente.name; //Me aseguro que el nombre del nuevo objeto instanciado sea el correcto

            Indice = 0;
            ObjetoEncontrado = false;
        }
        else
        {
            GameObject nuevoObjeto = Instantiate(HijoPadre, PadrePlayer.transform.position, HijoPadre.transform.rotation, PadrePlayer.transform); //Instancio el mismo objeto que llevaba el jugador
            nuevoObjeto.name = HijoPadre.name; //Me aseguro que el nombre sea el correcto

            Destroy(HijoPadre); //Destruyo el objeto que llevaba el jugador
        }

        interaccionAcriva = false;
        slider.gameObject.SetActive(false);
        HijoCortar = null;
    }
}
