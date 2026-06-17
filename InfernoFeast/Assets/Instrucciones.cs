using UnityEngine;

public class Instrucciones : MonoBehaviour
{
    public GameObject instrucciones;

    private void Start()
    {
        SetInstructions(false);
    }

    public void Abrir()
    {
        SetInstructions(true);
    }

    public void Cerrar()
    {
        SetInstructions(false);
    }

    private void SetInstructions(bool active)
    {
        if (instrucciones != null)
        {
            instrucciones.SetActive(active);
        }
        else
        {
            Debug.LogWarning("[Instrucciones] Falta asignar el panel de instrucciones.");
        }
    }
}