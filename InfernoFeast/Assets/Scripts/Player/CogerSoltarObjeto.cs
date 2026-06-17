using System.Collections;
using UnityEngine;

public class CogerSoltarObjeto : MonoBehaviour
{
    public GameObject Padre; // Lo usamos para comprobar si el jugador tiene un objeto en la mano.

    [Header("Sitios donde se puede dejar objetos")]
    public GameObject Encimera1;
    public GameObject Encimera2;

    public bool Hold, EncimeraSoltar, EncimeraCoger;

    private GameObject EncimeraCounter;
    public Animator animator;

    [Header("Interaccion")]
    [SerializeField] private float interactionCooldown = 0.1f;

    private bool interactionLocked = false;
    private Coroutine cooldownCoroutine;

    private void Update()
    {
        if (Padre == null)
            return;

        Hold = Padre.transform.childCount > 0;

        if (animator != null)
            animator.SetBool("HasObject", Hold);

        if (interactionLocked)
            return;

        if (!Input.GetKeyDown(KeyCode.E))
            return;

        if (EncimeraSoltar)
        {
            SoltarObjeto(EncimeraCounter);
            return;
        }

        if (EncimeraCoger)
        {
            CogerObjeto(EncimeraCounter);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Encimera"))
            return;

        EncimeraCounter = collision.gameObject;
        RefreshInteractionState();
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Encimera"))
            return;

        if (EncimeraCounter != collision.gameObject)
            return;

        EncimeraSoltar = false;
        EncimeraCoger = false;
        EncimeraCounter = null;

        StopCooldown();
    }

    private void RefreshInteractionState()
    {
        Hold = Padre != null && Padre.transform.childCount > 0;

        if (EncimeraCounter == null)
        {
            EncimeraSoltar = false;
            EncimeraCoger = false;
            return;
        }

        Transform counterParent = GetCounterParent(EncimeraCounter);
        bool counterHasObject = counterParent != null && counterParent.childCount > 0;

        EncimeraSoltar = Hold;
        EncimeraCoger = !Hold && counterHasObject;
    }

    private void SoltarObjeto(GameObject counter)
    {
        if (counter == null || Padre == null)
            return;

        if (Padre.transform.childCount <= 0)
            return;

        Transform counterParent = GetCounterParent(counter);
        if (counterParent == null)
            return;

        GameObject heldObject = Padre.transform.GetChild(0).gameObject;
        if (heldObject == null)
            return;

        if (counterParent.childCount == 0)
        {
            MoveObjectToParent(heldObject, counterParent, counterParent.position, heldObject.transform.rotation);
        }
        else
        {
            Encimera encimera = counter.GetComponent<Encimera>();
            if (encimera != null)
            {
                encimera.TryAddSecondObject(heldObject);
            }
        }

        StartInteractionCooldown();
    }

    private void CogerObjeto(GameObject counter)
    {
        if (counter == null || Padre == null)
            return;

        if (Padre.transform.childCount > 0)
            return;

        Transform counterParent = GetCounterParent(counter);
        if (counterParent == null || counterParent.childCount <= 0)
            return;

        GameObject objectOnCounter = counterParent.GetChild(0).gameObject;
        if (objectOnCounter == null)
            return;

        MoveObjectToParent(objectOnCounter, Padre.transform, Padre.transform.position, objectOnCounter.transform.rotation);
        objectOnCounter.transform.localPosition = Vector3.zero;

        StartInteractionCooldown();
    }

    private Transform GetCounterParent(GameObject counter)
    {
        if (counter == null || counter.transform.childCount <= 0)
            return null;

        return counter.transform.GetChild(0);
    }

    private void MoveObjectToParent(GameObject objectToMove, Transform newParent, Vector3 targetWorldPosition, Quaternion targetWorldRotation)
    {
        if (objectToMove == null || newParent == null)
            return;

        Vector3 originalWorldScale = objectToMove.transform.lossyScale;

        objectToMove.transform.SetParent(newParent, true);
        objectToMove.transform.position = targetWorldPosition;
        objectToMove.transform.rotation = targetWorldRotation;
        SetWorldScale(objectToMove.transform, originalWorldScale);

        PrepareRigidbody(objectToMove);
    }

    private void PrepareRigidbody(GameObject target)
    {
        if (target == null)
            return;

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb == null)
            return;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void SetWorldScale(Transform target, Vector3 worldScale)
    {
        if (target == null)
            return;

        if (target.parent == null)
        {
            target.localScale = worldScale;
            return;
        }

        Vector3 parentScale = target.parent.lossyScale;

        target.localScale = new Vector3(
            parentScale.x != 0f ? worldScale.x / parentScale.x : worldScale.x,
            parentScale.y != 0f ? worldScale.y / parentScale.y : worldScale.y,
            parentScale.z != 0f ? worldScale.z / parentScale.z : worldScale.z
        );
    }

    private void StartInteractionCooldown()
    {
        StopCooldown();
        cooldownCoroutine = StartCoroutine(InteractionCooldownRoutine());
    }

    private IEnumerator InteractionCooldownRoutine()
    {
        interactionLocked = true;
        EncimeraSoltar = false;
        EncimeraCoger = false;

        yield return new WaitForSeconds(interactionCooldown);

        interactionLocked = false;
        RefreshInteractionState();
        cooldownCoroutine = null;
    }

    private void StopCooldown()
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        interactionLocked = false;
    }
}