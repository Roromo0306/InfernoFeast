using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TableAnchor : MonoBehaviour
{
    public ClientTableGroup group; // asignar en inspector (el grupo cliente+mesa)
    public Transform snapPoint;    // punto donde quedará el plato (child)

    void Reset()
    {
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void Awake()
    {
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    // Usamos trigger: el plato al entrar se anclará
    private void OnTriggerEnter(Collider other)
    {
        Plate plate = other.GetComponent<Plate>();
        if (plate != null)
        {
            plate.HandleSnap(this);
        }
    }
}
