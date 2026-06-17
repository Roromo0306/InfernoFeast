using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutCounter : MonoBehaviour
{
    [Header("Slider")]
    public Slider slider;
    public KeyCode primeraTecla = KeyCode.Mouse0;
    public KeyCode segundaTecla = KeyCode.Mouse1;

    [Header("Padres")]
    public GameObject PadrePlayer;
    public GameObject PadreCortar;

    [Header("Listas")]
    public List<TipoIngrediente> cortados;
    public List<TipoIngrediente> ingredientes;

    [Header("Configuracion")]
    [SerializeField] private float pasosNecesarios = 8f;

    private bool interaccionActiva = false;
    private bool izquierdaClick = false;

    private int indice = 0;
    private bool objetoEncontrado = false;
    private GameObject hijoCortar;

    private void Awake()
    {
        PrepareSlider();
    }

    private void Update()
    {
        if (!interaccionActiva)
            return;

        HandleCutInput();
    }

    public void cortar()
    {
        if (!CanStartInteraction())
            return;

        GameObject hijoPadre = PadrePlayer.transform.GetChild(0).gameObject;
        if (hijoPadre == null)
            return;

        EstadoAlimento estadoAlimento = hijoPadre.GetComponent<EstadoAlimento>();
        if (estadoAlimento != null && IsBlockedFoodState(estadoAlimento.estado))
            return;

        BuscarIngrediente(hijoPadre.name);

        hijoCortar = hijoPadre;
        MoveObjectToParent(hijoCortar, PadreCortar.transform, PadreCortar.transform.position, hijoCortar.transform.rotation);

        Empezar();
    }

    private bool CanStartInteraction()
    {
        if (interaccionActiva)
            return false;

        if (PadrePlayer == null)
        {
            Debug.LogWarning("[CutCounter] Falta PadrePlayer en " + gameObject.name);
            return false;
        }

        if (PadreCortar == null)
        {
            Debug.LogWarning("[CutCounter] Falta PadreCortar en " + gameObject.name);
            return false;
        }

        if (PadrePlayer.transform.childCount <= 0)
            return false;

        if (PadreCortar.transform.childCount > 0)
            return false;

        return true;
    }

    private bool IsBlockedFoodState(int estado)
    {
        return estado == 1 || estado == 6 || estado == 7;
    }

    private void HandleCutInput()
    {
        if (!izquierdaClick)
        {
            if (Input.GetKeyDown(primeraTecla))
            {
                izquierdaClick = true;
                AddCutProgress();
            }
        }
        else
        {
            if (Input.GetKeyDown(segundaTecla))
            {
                izquierdaClick = false;
                AddCutProgress();
            }
        }
    }

    private void AddCutProgress()
    {
        if (slider == null)
            return;

        slider.value += 1f;

        if (slider.value >= slider.maxValue)
        {
            FinInteraccion();
        }
    }

    private void BuscarIngrediente(string nombreIngrediente)
    {
        indice = 0;
        objetoEncontrado = false;

        if (ingredientes == null)
            return;

        for (int i = 0; i < ingredientes.Count; i++)
        {
            if (ingredientes[i] != null && ingredientes[i].name == nombreIngrediente)
            {
                indice = i;
                objetoEncontrado = true;
                return;
            }
        }
    }

    private void Empezar()
    {
        interaccionActiva = true;
        izquierdaClick = false;

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = Mathf.Max(1f, pasosNecesarios);
            slider.value = 0f;
            slider.gameObject.SetActive(true);
        }
    }

    private void FinInteraccion()
    {
        if (hijoCortar == null)
        {
            ResetInteraction();
            return;
        }

        if (objetoEncontrado)
        {
            TipoIngrediente ingredienteCortado = GetProcessedIngredient();

            Destroy(hijoCortar);
            hijoCortar = null;

            if (ingredienteCortado != null && ingredienteCortado.prefabIngrediente != null)
            {
                InstantiateInPlayerHand(ingredienteCortado.prefabIngrediente);
            }
        }
        else
        {
            MoveObjectToParent(hijoCortar, PadrePlayer.transform, PadrePlayer.transform.position, hijoCortar.transform.rotation);
            hijoCortar.transform.localPosition = Vector3.zero;
            hijoCortar = null;
        }

        ResetInteraction();
    }

    private TipoIngrediente GetProcessedIngredient()
    {
        if (!objetoEncontrado)
            return null;

        if (cortados == null)
            return null;

        if (indice < 0 || indice >= cortados.Count)
            return null;

        return cortados[indice];
    }

    private void InstantiateInPlayerHand(GameObject prefab)
    {
        if (prefab == null || PadrePlayer == null)
            return;

        Vector3 prefabWorldScale = prefab.transform.lossyScale;

        GameObject nuevoObjeto = Instantiate(prefab, PadrePlayer.transform.position, prefab.transform.rotation);
        nuevoObjeto.name = prefab.name;

        nuevoObjeto.transform.SetParent(PadrePlayer.transform, true);
        SetWorldScale(nuevoObjeto.transform, prefabWorldScale);
        nuevoObjeto.transform.localPosition = Vector3.zero;

        PrepareRigidbody(nuevoObjeto);
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

    private void PrepareSlider()
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = Mathf.Max(1f, pasosNecesarios);
        slider.value = 0f;
        slider.gameObject.SetActive(false);
    }

    private void ResetInteraction()
    {
        interaccionActiva = false;
        izquierdaClick = false;
        indice = 0;
        objetoEncontrado = false;
        hijoCortar = null;

        if (slider != null)
        {
            slider.value = 0f;
            slider.gameObject.SetActive(false);
        }
    }
}