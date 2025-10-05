using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasCajas : MonoBehaviour
{
    public Button Cerrar;

    public List<Image> botones;

    public void CerrarUI()
    {
        this.gameObject.SetActive(false);

        //Activamos todos los botones para la proxima vez que se abra
        for(int i = 0; i < botones.Count; i++)
        {
            botones[i].gameObject.SetActive(true);
        }
    }
}
