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

    [Header("Anzuelo")]
    public GameObject Anzuelo;

    [Header("Variables para congelar al player")]
    public Rigidbody PlayerRB;
    public PlayerController playerController;

    private float DistanciaLanzamiento = 0f;
    [HideInInspector] public bool Lanzar = true;

    //LanzamientoPower, MaxPower, DistanciaLanzamiento

    void Update()
    {
        if (EnArea && Lanzar)
        {
            if (Input.GetKey(KeyCode.E) && LanzamientoPower <= MaxPower) //Pulsa para lanzar el anzuelo
            {
                LanzamientoPower += RatioCarga * Time.deltaTime;
            }

            if (Input.GetKeyUp(KeyCode.E)) //Lo lanza
            {
                DistanciaLanzamiento = Mathf.Round( (LanzamientoPower / MaxPower) * 10f) / 10f;
                LanzamientoPower = 0f;

                PlayerRB.constraints = RigidbodyConstraints.FreezeAll;
                playerController.enabled = false;

                Anzuelo.SetActive(true);
                Lanzar = false;
            }
        }

        if(EnArea && !Lanzar)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(RecogerCana());
            }
        }
    }

    IEnumerator RecogerCana()
    {
        PlayerRB.constraints = RigidbodyConstraints.None;
        PlayerRB.constraints = RigidbodyConstraints.FreezeRotation;
        playerController.enabled = true;

        Anzuelo.SetActive(false);

        yield return new WaitForSeconds(1f);
        Lanzar = true;

        StopAllCoroutines();
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
