using System.Collections;
using UnityEngine;

public class CogerSoltarObjeto : MonoBehaviour
{
    public GameObject Padre; // Lo usaremos para verificar si tiene hijos y por tanto si el jugador tiene un objeto

    [Header("Sitios donde se puede dejar objetos")]
    public GameObject Encimera1;
    public GameObject Encimera2;

    public bool Hold, EncimeraSoltar, EncimeraCoger;

    private GameObject EncimeraCounter;
    public Animator animator;

    private void Update()
    {
        if (Padre == null)
            return;

        Hold = Padre.transform.childCount > 0;

        if (animator != null)
            animator.SetBool("HasObject", Hold);

        if (EncimeraSoltar && Input.GetKeyDown(KeyCode.E))
        {
            SoltarObjeto(EncimeraCounter);
        }

        if (EncimeraCoger && Input.GetKeyDown(KeyCode.E))
        {
            CogerObjeto(EncimeraCounter);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Encimera"))
            return;

        EncimeraCounter = collision.gameObject;

        if (Hold)
        {
            EncimeraSoltar = true;
            EncimeraCoger = false;
        }
        else
        {
            EncimeraCoger = true;
            EncimeraSoltar = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Encimera"))
            return;

        if (EncimeraCounter == collision.gameObject)
        {
            EncimeraSoltar = false;
            EncimeraCoger = false;
            EncimeraCounter = null;
        }

        StopAllCoroutines();
    }

    // Funcion para soltar el objeto en una encimera
    private void SoltarObjeto(GameObject collision)
    {
        StopAllCoroutines();

        if (collision == null || Padre == null)
            return;

        if (Padre.transform.childCount <= 0)
            return;

        if (collision.transform.childCount <= 0)
            return;

        GameObject objeto = Padre.transform.GetChild(0).gameObject;
        GameObject PadreEncimera = collision.transform.GetChild(0).gameObject;

        if (objeto == null || PadreEncimera == null)
            return;

        if (PadreEncimera.transform.childCount == 0)
        {
            Vector3 originalWorldScale = objeto.transform.lossyScale;

            GameObject newObj = Instantiate(objeto, PadreEncimera.transform.position, objeto.transform.rotation);
            newObj.name = newObj.name.Replace("(Clone)", "").Trim();

            newObj.transform.SetParent(PadreEncimera.transform, true);
            SetWorldScale(newObj.transform, originalWorldScale);

            Rigidbody rbNew = newObj.GetComponent<Rigidbody>();
            if (rbNew != null)
            {
                rbNew.isKinematic = true;
                rbNew.useGravity = false;
                rbNew.velocity = Vector3.zero;
                rbNew.angularVelocity = Vector3.zero;
            }

            Destroy(objeto);
        }
        else
        {
            Encimera enci = collision.GetComponent<Encimera>();

            if (enci != null)
            {
                enci.objeto2 = objeto;
            }
        }

        StartCoroutine(TempCoger());
    }

    private void CogerObjeto(GameObject collision)
    {
        StopAllCoroutines();

        if (collision == null || Padre == null)
            return;

        if (collision.transform.childCount <= 0)
            return;

        GameObject PadreEncimera = collision.transform.GetChild(0).gameObject;

        if (PadreEncimera == null)
            return;

        if (PadreEncimera.transform.childCount > 0)
        {
            GameObject objeto = PadreEncimera.transform.GetChild(0).gameObject;

            if (objeto == null)
                return;

            Vector3 originalWorldScale = objeto.transform.lossyScale;

            GameObject newObj = Instantiate(objeto, Padre.transform.position, objeto.transform.rotation);
            newObj.name = newObj.name.Replace("(Clone)", "").Trim();

            newObj.transform.SetParent(Padre.transform, true);
            SetWorldScale(newObj.transform, originalWorldScale);

            Rigidbody rbNew = newObj.GetComponent<Rigidbody>();
            if (rbNew != null)
            {
                rbNew.isKinematic = true;
                rbNew.useGravity = false;
                rbNew.velocity = Vector3.zero;
                rbNew.angularVelocity = Vector3.zero;
            }

            newObj.transform.localPosition = Vector3.zero;

            Destroy(objeto);
        }

        StartCoroutine(TempSoltar());
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

    IEnumerator TempCoger()
    {
        yield return new WaitForSeconds(0.1f);
        EncimeraCoger = true;
        EncimeraSoltar = false;
        StopAllCoroutines();
    }

    IEnumerator TempSoltar()
    {
        yield return new WaitForSeconds(0.1f);
        EncimeraCoger = false;
        EncimeraSoltar = true;
        StopAllCoroutines();
    }
}