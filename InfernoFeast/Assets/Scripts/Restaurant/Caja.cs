using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Caja : MonoBehaviour
{
    [Header("Listas de ingredientes y para la UI")]
    public List<TipoIngrediente> Ingredientes;
    public List<Sprite> ImagenesUI;
    public List<Image> Botones;
    public List<TextMeshProUGUI> textos;

    public CanvasCajas canvascajas;

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.E))
            {
                //Con esto desactivamos en el caso de que nos sobre un boton
                if (ImagenesUI.Count < Botones.Count)
                {
                    Botones[Botones.Count -1].gameObject.SetActive(false);
                }

                //Asociamos cada imagen a un boton
                for (int i = 0; i < ImagenesUI.Count; i++)
                {
                    Botones[i].sprite = ImagenesUI[i];
                    textos[i].text = Ingredientes[i].name;
                }

                canvascajas.SetTipos(new List<TipoIngrediente>(Ingredientes)); //Pasa los ingredientes a CanvasCaja
                canvascajas.gameObject.SetActive(true); //Activo el canva de los botones

            }
        }
    }
}
