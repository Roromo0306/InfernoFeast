using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MixCounter : MonoBehaviour
{
    [Header("Slider")]
    public Slider progressBar;
    public float progressIncrement = 1f;
    public float maxProgress = 100f;

    [Header("Sensibilidad de Movimiento")]
    public float circleThreshold = 0.5f;
    public float rotationThreshold = 10.8f;

    [Header("Padres")]
    public GameObject PadrePlayer;
    public GameObject PadreMix;

    [Header("Listas")]
    public List<TipoIngrediente> batidos;
    public List<TipoIngrediente> ingredientes;

    private float currentProgress = 0f;
    private bool isInteracting = false;
    private Vector2 lastMouseDir;
    private float accumulatedRotation = 0f;

    private GameObject hijoMix;
    private int indice = 0;
    private bool objetoEncontrado = false;

    private void Awake()
    {
        PrepareProgressBar();
    }

    private void Update()
    {
        if (!isInteracting)
            return;

        HandleMixInput();
    }

    public void StartMixing()
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

        hijoMix = hijoPadre;
        MoveObjectToParent(hijoMix, PadreMix.transform, PadreMix.transform.position, hijoMix.transform.rotation);

        BeginMixingInteraction();
    }

    private bool CanStartInteraction()
    {
        if (isInteracting)
            return false;

        if (PadrePlayer == null)
        {
            Debug.LogWarning("[MixCounter] Falta PadrePlayer en " + gameObject.name);
            return false;
        }

        if (PadreMix == null)
        {
            Debug.LogWarning("[MixCounter] Falta PadreMix en " + gameObject.name);
            return false;
        }

        if (PadrePlayer.transform.childCount <= 0)
            return false;

        if (PadreMix.transform.childCount > 0)
            return false;

        return true;
    }

    private bool IsBlockedFoodState(int estado)
    {
        return estado == 5 || estado == 6 || estado == 7;
    }

    private void HandleMixInput()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            if (mouseDelta.magnitude > circleThreshold)
            {
                Vector2 currentDir = mouseDelta.normalized;

                if (lastMouseDir != Vector2.zero)
                {
                    float angle = Vector2.SignedAngle(lastMouseDir, currentDir);
                    accumulatedRotation += Mathf.Abs(angle);

                    if (accumulatedRotation >= rotationThreshold)
                    {
                        AddProgress();
                        accumulatedRotation = 0f;
                    }
                }

                lastMouseDir = currentDir;
            }
        }
        else
        {
            lastMouseDir = Vector2.zero;
            accumulatedRotation = 0f;
        }
    }

    private void AddProgress()
    {
        currentProgress += progressIncrement;
        currentProgress = Mathf.Clamp(currentProgress, 0f, Mathf.Max(1f, maxProgress));

        if (progressBar != null)
            progressBar.value = currentProgress / Mathf.Max(1f, maxProgress);

        if (currentProgress >= maxProgress)
        {
            EndMixing();
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

    private void BeginMixingInteraction()
    {
        currentProgress = 0f;
        accumulatedRotation = 0f;
        lastMouseDir = Vector2.zero;
        isInteracting = true;

        if (progressBar != null)
        {
            progressBar.value = 0f;
            progressBar.gameObject.SetActive(true);
        }
    }

    private void EndMixing()
    {
        if (hijoMix == null)
        {
            ResetInteraction();
            return;
        }

        if (objetoEncontrado)
        {
            TipoIngrediente ingredienteBatido = GetProcessedIngredient();

            Destroy(hijoMix);
            hijoMix = null;

            if (ingredienteBatido != null && ingredienteBatido.prefabIngrediente != null)
            {
                InstantiateInPlayerHand(ingredienteBatido.prefabIngrediente);
            }
        }
        else
        {
            MoveObjectToParent(hijoMix, PadrePlayer.transform, PadrePlayer.transform.position, hijoMix.transform.rotation);
            hijoMix.transform.localPosition = Vector3.zero;
            hijoMix = null;
        }

        ResetInteraction();
    }

    private TipoIngrediente GetProcessedIngredient()
    {
        if (!objetoEncontrado)
            return null;

        if (batidos == null)
            return null;

        if (indice < 0 || indice >= batidos.Count)
            return null;

        return batidos[indice];
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

    private void PrepareProgressBar()
    {
        if (progressBar == null)
            return;

        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.value = 0f;
        progressBar.gameObject.SetActive(false);
    }

    private void ResetInteraction()
    {
        currentProgress = 0f;
        isInteracting = false;
        lastMouseDir = Vector2.zero;
        accumulatedRotation = 0f;
        indice = 0;
        objetoEncontrado = false;
        hijoMix = null;

        if (progressBar != null)
        {
            progressBar.value = 0f;
            progressBar.gameObject.SetActive(false);
        }
    }
}