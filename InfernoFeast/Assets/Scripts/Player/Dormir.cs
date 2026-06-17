using UnityEngine;

public class Dormir : MonoBehaviour
{
    public GameObject EndDayPannel;

    [HideInInspector] public bool EnContacto = false;
    [HideInInspector] public bool nuevoDia = false;

    private void Start()
    {
        if (EndDayPannel != null)
            EndDayPannel.SetActive(false);
    }

    private void Update()
    {
        if (!EnContacto)
            return;

        if (nuevoDia)
            return;

        if (!Input.GetKeyDown(KeyCode.E))
            return;

        nuevoDia = true;
        NuevoDia();
    }

    private void NuevoDia()
    {
        if (EndDayPannel != null)
        {
            EndDayPannel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[Dormir] Falta EndDayPannel en " + gameObject.name);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name == "Player")
        {
            EnContacto = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name == "Player")
        {
            EnContacto = false;
        }
    }
}