using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitRestaurant : MonoBehaviour
{
    private GameObject mapPanel;
    private GameObject exitPanel;

    // Start is called before the first frame update
    void Start()
    {
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
        if(collider.tag == "Player")
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
