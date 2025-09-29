using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private GameObject mapPanel;

    // Start is called before the first frame update
    void Start()
    {
        mapPanel = GameObject.Find("Map");
        mapPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider collider)
    {
        if(collider.tag == "Player")
        {
            mapPanel.SetActive(true);
        }
    }
}
