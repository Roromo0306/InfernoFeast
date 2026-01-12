using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TableAnchor : MonoBehaviour
{
    public ClientTableGroup group;

    // Usamos el snapPoint del grupo (UNA sola referencia)
    public Transform SnapPoint => group != null ? group.snapPoint : null;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger detectado por: " + other.name);

        Plate plate = other.GetComponent<Plate>();

        if (plate != null)
        {
            Debug.Log("Es un plato, haciendo snap");
            plate.HandleSnap(this);
        }
    }
}

