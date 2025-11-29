using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgrsionPesca : MonoBehaviour
{
    [Header("PescaUI")]
    public Image Barra;
    public Slider BarraProgrso;

    [Header("GameObjects")]
    public GameObject Player;


    private bool EmpezarPesca = false;
    void Start()
    {
        
    }

    void Update()
    {
        LanzarCanna lan = Player.GetComponent<LanzarCanna>();

        if (!lan.Lanzar)
        {
            //Activo todo lo de la barra
            Barra.gameObject.SetActive(true);
            BarraProgrso.gameObject.SetActive(true);


            EmpezarPesca = true;
        }
    }
}
