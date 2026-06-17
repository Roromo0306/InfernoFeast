using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CanvasCajas : MonoBehaviour
{
    public Transform EspacioInstanciado;
    public List<Image> botones;
    public CanvasCajas canvascajas;

    public List<TipoIngrediente> tipos = new List<TipoIngrediente>(); // Aqui se guardan las listas de las cajas.

    [Header("Botones Canvas")]
    public Button Imagen1;
    public Button Imagen2;
    public Button Imagen3;

    private bool buttonsAssociated = false;

    private void Awake()
    {
        AsociarBotones();
    }

    private void OnEnable()
    {
        RefreshButtonsState();
    }

    // Esta funcion es llamada cuando se interactua con una caja y sirve para guardar los scriptable objects activos en este canvas.
    public void SetTipos(List<TipoIngrediente> nuevosTipos)
    {
        if (tipos == null)
            tipos = new List<TipoIngrediente>();

        tipos.Clear();

        if (nuevosTipos == null)
            return;

        for (int i = 0; i < nuevosTipos.Count; i++)
        {
            tipos.Add(nuevosTipos[i]);
        }

        RefreshButtonsState();
    }

    // Esta funcion cierra la UI y restaura todo.
    public void CerrarUI()
    {
        if (botones != null)
        {
            for (int i = 0; i < botones.Count; i++)
            {
                if (botones[i] != null)
                    botones[i].gameObject.SetActive(true);
            }
        }

        if (tipos != null)
            tipos.Clear();

        gameObject.SetActive(false);
    }

    // Mantengo el nombre original con la errata para no romper referencias del inspector.
    public void IntanciarIngrediente(List<TipoIngrediente> TipoIngrediente)
    {
        int selectedIndex = GetSelectedButtonIndex();
        InstanciarIngredientePorIndice(selectedIndex);
    }

    public void InstanciarIngredientePorIndice(int indice)
    {
        if (indice < 0)
            return;

        if (EspacioInstanciado == null)
        {
            Debug.LogWarning("[CanvasCajas] Falta EspacioInstanciado.");
            return;
        }

        if (EspacioInstanciado.childCount > 0)
        {
            CerrarUI();
            return;
        }

        if (tipos == null || indice >= tipos.Count || tipos[indice] == null)
        {
            Debug.LogWarning("[CanvasCajas] No existe ingrediente para el indice " + indice + ".");
            return;
        }

        GameObject prefab = tipos[indice].prefabIngrediente;
        if (prefab == null)
        {
            Debug.LogWarning("[CanvasCajas] El ingrediente " + tipos[indice].name + " no tiene prefab asignado.");
            return;
        }

        CreateIngredientInPlayerHand(prefab);
        CerrarUI();
    }

    public void RefreshButtonsState()
    {
        if (botones == null)
            return;

        for (int i = 0; i < botones.Count; i++)
        {
            if (botones[i] == null)
                continue;

            bool hasIngredient = tipos != null && i < tipos.Count && tipos[i] != null;
            botones[i].gameObject.SetActive(hasIngredient);
        }
    }

    // Con esta funcion asociamos los botones a la funcion de instanciar ingredientes.
    private void AsociarBotones()
    {
        if (buttonsAssociated)
            return;

        AssignButton(Imagen1, 0);
        AssignButton(Imagen2, 1);
        AssignButton(Imagen3, 2);

        buttonsAssociated = true;
    }

    private void AssignButton(Button button, int index)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => InstanciarIngredientePorIndice(index));
    }

    private int GetSelectedButtonIndex()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
            return -1;

        GameObject selectedButton = EventSystem.current.currentSelectedGameObject;

        if (Imagen1 != null && selectedButton == Imagen1.gameObject)
            return 0;

        if (Imagen2 != null && selectedButton == Imagen2.gameObject)
            return 1;

        if (Imagen3 != null && selectedButton == Imagen3.gameObject)
            return 2;

        if (botones != null)
        {
            for (int i = 0; i < botones.Count; i++)
            {
                if (botones[i] != null && selectedButton == botones[i].gameObject)
                    return i;
            }
        }

        return -1;
    }

    private void CreateIngredientInPlayerHand(GameObject prefab)
    {
        Vector3 prefabWorldScale = prefab.transform.lossyScale;

        GameObject newIngredient = Instantiate(prefab, EspacioInstanciado.position, prefab.transform.rotation);
        newIngredient.name = prefab.name;

        newIngredient.transform.SetParent(EspacioInstanciado, true);
        SetWorldScale(newIngredient.transform, prefabWorldScale);
        newIngredient.transform.localPosition = Vector3.zero;

        PrepareRigidbody(newIngredient);
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