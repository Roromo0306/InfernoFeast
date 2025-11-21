using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanzarCanna : MonoBehaviour
{
    [Header("AreaPesca")]
    public GameObject AreaPesca;
    public bool EnArea = false;

    [Header("Variables Lanzamiento")]
    public float LanzamientoPower = 0f;
    public float MaxPower = 10f;
    public float RatioCarga = 5f;
    public float DistanciaLanzamiento = 0f;

    private bool Lanzar = true;

    //LanzamientoPower, MaxPower, DistanciaLanzamiento
    void Start()
    {
        
    }

    void Update()
    {
        if (EnArea && Lanzar)
        {
            if (Input.GetKey(KeyCode.E) && LanzamientoPower <= MaxPower)
            {
                LanzamientoPower += RatioCarga * Time.deltaTime;
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                DistanciaLanzamiento = Mathf.Round( (LanzamientoPower / MaxPower) * 10f) / 10f;
                LanzamientoPower = 0f;
            }
        }
    }

    private void LanzarCana()
    {
        Debug.Log("Lanzar");
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject == AreaPesca)
        {
            EnArea = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == AreaPesca)
        {
            EnArea = false;
        }
    }
}
