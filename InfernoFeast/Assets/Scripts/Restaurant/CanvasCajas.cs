using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CanvasCajas : MonoBehaviour
{
    public Transform EspacioInstanciado;
    public List<Image> botones;
    public CanvasCajas canvascajas;

    public List<TipoIngrediente> tipos; //Aqui se guardaran las listas de las cajas
    private Button Cerrar;

    [Header("Botones Canvas")]
    public Button Imagen1;
    public Button Imagen2;
    public Button Imagen3;

    private void Start()
    {
        AsociarBotones();
    }

    //Esta fucion es llamada cuando se interactua con una caja y sirve para llevar toda la lista de scriptable objects a la lista de este script
    public void SetTipos(List<TipoIngrediente> tipos)
    {
        this.tipos = tipos;
    }

    //Esta funcion cierra la UI y restaura todo
    public void CerrarUI()
    {
        this.gameObject.SetActive(false);

        //Activamos todos los botones para la proxima vez que se abra
        for(int i = 0; i < botones.Count; i++)
        {
            botones[i].gameObject.SetActive(true);
        }

        tipos.Clear();
    }

    //Con esta funcion instanciamos el ingrediente que escojamos en los botones
    public void IntanciarIngrediente(List<TipoIngrediente> TipoIngrediente)
    {
        int indice = 0; //Aqui se guardara el indice del boton pulsado
        GameObject BotonPulsado = EventSystem.current.currentSelectedGameObject; //Esto guarda el gameobject del boton que se acaba de pulsar

        for(int i = 0; i < botones.Count; i++)
        {
            if (BotonPulsado.name == botones[i].name)
            {
                indice = i;
                break;
            }
        }

        Instantiate(tipos[indice].prefabIngrediente, EspacioInstanciado.position, EspacioInstanciado.rotation, EspacioInstanciado); //Se Instancia los ingredientes
        CerrarUI();
    }

    //Con esta funcion asociamos los tres botones a la funcion Instaciar Ingredientes
    private void AsociarBotones()
    {
        Imagen1.onClick.AddListener(() =>
        {
            canvascajas.IntanciarIngrediente(tipos);
        }
        );

        Imagen2.onClick.AddListener(() =>
        {
            canvascajas.IntanciarIngrediente(tipos);
        }
        );

        Imagen3.onClick.AddListener(() =>
        {
            canvascajas.IntanciarIngrediente(tipos);
        }
        );
    }
}
