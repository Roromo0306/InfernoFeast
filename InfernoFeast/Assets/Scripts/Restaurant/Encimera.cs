using System.Collections.Generic;
using UnityEngine;

public class Encimera : MonoBehaviour
{
    public List<RecetasSO> recetas;
    public GameObject PadreEncimera, objeto1, objeto2 = null;
    public TipoIngrediente mezcla;
    public bool TieneObjeto = false, EncontradoPareja = true;

    [Header("Opciones de recetas")]
    [SerializeField] private bool permitirRecetasEnCualquierOrden = true;

    private bool procesandoReceta = false;

    private void Update()
    {
        UpdateObjectState();

        if (!procesandoReceta && objeto2 != null && objeto1 != null)
        {
            Recetas();
        }
    }

    public bool TryAddSecondObject(GameObject secondObject)
    {
        if (secondObject == null)
            return false;

        if (procesandoReceta)
            return false;

        UpdateObjectState();

        if (objeto1 == null)
            return false;

        if (objeto1 == secondObject)
            return false;

        if (objeto2 != null)
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

        procesandoReceta = true;
        EncontradoPareja = false;

        RecetasSO recetaEncontrada = BuscarReceta(objeto1, objeto2);

        if (recetaEncontrada != null && recetaEncontrada.Resultado != null && recetaEncontrada.Resultado.prefabIngrediente != null)
        {
            GameObject resultPrefab = recetaEncontrada.Resultado.prefabIngrediente;
            Quaternion resultRotation = resultPrefab.transform.rotation;

            Destroy(objeto1);
            Destroy(objeto2);

            InstantiateResult(resultPrefab, resultRotation);

            LimpiarEstadoReceta(true);
            return;
        }

        Destroy(objeto1);
        Destroy(objeto2);

        if (mezcla != null && mezcla.prefabIngrediente != null)
        {
            Quaternion mixRotation = PadreEncimera != null ? PadreEncimera.transform.rotation : Quaternion.identity;
            InstantiateResult(mezcla.prefabIngrediente, mixRotation);
        }

        LimpiarEstadoReceta(false);
    }

    private RecetasSO BuscarReceta(GameObject firstObject, GameObject secondObject)
    {
        if (recetas == null)
            return null;

        for (int i = 0; i < recetas.Count; i++)
        {
            RecetasSO receta = recetas[i];
            if (receta == null)
                continue;

            if (receta.Ingrediente1 == null || receta.Ingrediente2 == null || receta.Resultado == null)
                continue;

            bool directMatch = IngredientMatches(receta.Ingrediente1, firstObject) && IngredientMatches(receta.Ingrediente2, secondObject);
            if (directMatch)
                return receta;

            if (permitirRecetasEnCualquierOrden)
            {
                bool reverseMatch = IngredientMatches(receta.Ingrediente1, secondObject) && IngredientMatches(receta.Ingrediente2, firstObject);
                if (reverseMatch)
                    return receta;
            }
        }

        return null;
    }

    private bool IngredientMatches(TipoIngrediente ingredientType, GameObject objectToCheck)
    {
        if (ingredientType == null || objectToCheck == null)
            return false;

        string objectName = NormalizeName(objectToCheck.name);
        string scriptableName = NormalizeName(ingredientType.name);

        if (objectName == scriptableName)
            return true;

        if (ingredientType.prefabIngrediente != null)
        {
            string prefabName = NormalizeName(ingredientType.prefabIngrediente.name);
            if (objectName == prefabName)
                return true;
        }

        return false;
    }

    private string NormalizeName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
            return string.Empty;

        return rawName.Replace("(Clone)", string.Empty).Trim();
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

    private void LimpiarEstadoReceta(bool recetaEncontrada)
    {
        objeto1 = null;
        objeto2 = null;
        TieneObjeto = PadreEncimera != null && PadreEncimera.transform.childCount > 0;
        EncontradoPareja = recetaEncontrada;
        procesandoReceta = false;
    }
}