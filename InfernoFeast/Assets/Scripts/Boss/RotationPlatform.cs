using UnityEngine;

public class RotationPlatform : MonoBehaviour
{
    [Header("Rotación")]
    public Vector3 rotationAxis = Vector3.up; // Eje de rotación (por defecto Y)
    public float rotationSpeed = 50f;         // Velocidad de rotación en grados/segundo

    private void Update()
    {
        RotacionPlataforma();
    }
    public void RotacionPlataforma()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}