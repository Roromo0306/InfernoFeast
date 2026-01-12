using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Plate : MonoBehaviour
{
    public TipoIngrediente dish; // plato final (vieiras, etc.)

    private bool hasSnapped = false;

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

        Transform snap = anchor.SnapPoint;

        if (snap != null)
        {
            transform.SetParent(snap);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        GameRoundsManager.Instance?.OnPlateDelivered(anchor, this);
    }
}
