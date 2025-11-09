using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Plate : MonoBehaviour
{
    public TipoIngrediente dish; // asigna aquí el SO del plato (vieira, etc.)
    bool hasSnapped = false;

    public float snapDelay = 0.02f; // pequeño delay para evitar interferencias de física

    public void HandleSnap(TableAnchor anchor)
    {
        if (hasSnapped) return;
        hasSnapped = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // mover y parentear al snapPoint
        if (anchor != null && anchor.snapPoint != null)
        {
            transform.position = anchor.snapPoint.position;
            transform.rotation = anchor.snapPoint.rotation;
            transform.SetParent(anchor.snapPoint, true);
        }

        // avisar al GameRoundsManager (singleton)
        GameRoundsManager.Instance?.OnPlateDelivered(anchor, this);
    }
}
