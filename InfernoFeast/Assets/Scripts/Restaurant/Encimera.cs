using System.Collections.Generic;
using UnityEngine;

public class Encimera : MonoBehaviour
{
    public List<RecetasSO> recetas;
    public GameObject PadreEncimera, objeto1, objeto2 = null;
    public TipoIngrediente mezcla;
    public bool TieneObjeto = false, EncontradoPareja = true;

    private void Update()
    {
        UpdateObjectState();

        if (objeto2 != null && objeto1 != null)
        {
            Recetas();
        }
    }

    public bool TryAddSecondObject(GameObject secondObject)
    {
        if (secondObject == null)
            return false;

        UpdateObjectState();

        if (objeto1 == null)
            return false;

        objeto2 = secondObject;
        Recetas();
        return true;
    }

    private void UpdateObjectState()
    {
        if (PadreEncimera == null)
        {
            objeto1 = null;
            TieneObjeto = false;
            return;
        }

        if (PadreEncimera.transform.childCount > 0)
        {
            objeto1 = PadreEncimera.transform.GetChild(0).gameObject;
            TieneObjeto = true;
        }
        else
        {
            objeto1 = null;
            TieneObjeto = false;
        }
    }

    private void Recetas()
    {
        if (objeto1 == null || objeto2 == null)
            return;

        EncontradoPareja = false;

        if (recetas != null)
        {
            for (int i = 0; i < recetas.Count; i++)
            {
                if (recetas[i] == null)
                    continue;

                if (recetas[i].Ingrediente1 == null || recetas[i].Ingrediente2 == null || recetas[i].Resultado == null)
                    continue;

                if (recetas[i].Ingrediente1.name == objeto1.name && recetas[i].Ingrediente2.name == objeto2.name)
                {
                    GameObject resultPrefab = recetas[i].Resultado.prefabIngrediente;
                    Quaternion resultRotation = resultPrefab != null ? resultPrefab.transform.rotation : Quaternion.identity;

                    Destroy(objeto1);
                    Destroy(objeto2);

                    InstantiateResult(resultPrefab, resultRotation);

                    objeto1 = null;
                    objeto2 = null;
                    EncontradoPareja = true;
                    return;
                }
            }
        }

        if (!EncontradoPareja)
        {
            Destroy(objeto1);
            Destroy(objeto2);

            if (mezcla != null && mezcla.prefabIngrediente != null)
            {
                Quaternion mixRotation = PadreEncimera != null ? PadreEncimera.transform.rotation : Quaternion.identity;
                InstantiateResult(mezcla.prefabIngrediente, mixRotation);
            }

            objeto1 = null;
            objeto2 = null;
        }
    }

    private void InstantiateResult(GameObject prefab, Quaternion rotation)
    {
        if (prefab == null || PadreEncimera == null)
            return;

        Vector3 prefabWorldScale = prefab.transform.lossyScale;

        GameObject instantiated = Instantiate(prefab, PadreEncimera.transform.position, rotation);
        instantiated.name = prefab.name;

        instantiated.transform.SetParent(PadreEncimera.transform, true);
        SetWorldScale(instantiated.transform, prefabWorldScale);
        PrepareRigidbody(instantiated);
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
}