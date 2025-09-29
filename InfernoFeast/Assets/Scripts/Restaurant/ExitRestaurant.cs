using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitRestaurant : MonoBehaviour
{
    //Variables que hacen referencia a los paneles del canvas dentro del juego
    private GameObject mapPanel;
    private GameObject exitPanel;

    // Start is called before the first frame update
    void Start()
    {
        //Buscamos los objetos y los desactivamos
        mapPanel = GameObject.Find("Map");
        mapPanel.SetActive(false);
        
        exitPanel = GameObject.Find("Exit Restaurant Panel");
        exitPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider collider)
    {
        if(collider.tag == "Player") //Si el player entra en el collider activa el panel
        {
            exitPanel.SetActive(true);
        }
    }

    public void Yes()
    {
        exitPanel.SetActive(false);
        mapPanel.SetActive(true);
    }

    public void No()
    {
        exitPanel.SetActive(false);
    }
}
