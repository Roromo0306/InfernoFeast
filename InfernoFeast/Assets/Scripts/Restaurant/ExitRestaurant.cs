using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void Restaurant()
    {
        mapPanel.SetActive(false);
        exitPanel.SetActive(false);
    }

    public void Market()
    {
        SceneManager.LoadScene("Market");
    }

    public void Boss1()
    {
        SceneManager.LoadScene("Boss 1");
    }

    public void FishingLake()
    {
        SceneManager.LoadScene("Fishing Lake");
    }

    public void Farm()
    {
        SceneManager.LoadScene("Farm");
    }
}
