using UnityEngine;

public class RotationPlatform : MonoBehaviour
{
    [Header("Rotaci�n")]
    public Vector3 rotationAxis = Vector3.up; // Eje de rotaci�n (por defecto Y)
    public float rotationSpeed = 50f;         // Velocidad de rotaci�n en grados/segundo

    private void Update()
    {
        RotacionPlataforma();
    }
    public void RotacionPlataforma()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}