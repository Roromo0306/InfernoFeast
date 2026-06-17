using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class CookingCounterBase : MonoBehaviour
{
    [Header("Padres")]
    public GameObject PadrePlayer;

    [Header("Listas")]
    public List<TipoIngrediente> ingredientes;

    [Header("UI")]
    public Slider slider;
    public float duracion = 7f;

    [Header("Interaccion")]
    public InteractuarCounter counterInt;
    public TipoIngrediente Quemado;
    public Image QuemadoImage;

    [Header("Audios")]
    public AudioSource audio;
    public AudioSource audioQuemado;

    [Header("Estado")]
    public bool quemado = false;

    private int indice;
    private bool objetoEncontrado = false;
    private Coroutine corrutina = null;
    private bool haSonado = false;
    private bool haSonado2 = false;

    protected abstract string LogName { get; }
    protected abstract GameObject StationParent { get; }
    protected abstract List<TipoIngrediente> ProcessedIngredients { get; }
    protected abstract bool IsPlayerInteractingWithThisCounter { get; }
    protected abstract bool IsBlockedFoodState(int estado);

    protected virtual bool CanTakeFood(float progress)
    {
        return progress >= 0.6f && progress <= 0.9f;
    }

    protected virtual void OnProcessStarted()
    {
    }

    protected virtual void StopExtraAudio()
    {
    }

    protected virtual void Awake()
    {
        HideSlider();

        if (QuemadoImage != null)
        {
            QuemadoImage.enabled = false;
        }
    }

    protected virtual void Update()
    {
        TryTakeBurnedFood();
    }

    protected void StartCookingProcess()
    {
        if (!HasRequiredReferences())
            return;

        if (corrutina != null || quemado)
            return;

        if (PadrePlayer.transform.childCount <= 0)
            return;

        GameObject playerFood = PadrePlayer.transform.GetChild(0).gameObject;
        if (playerFood == null)
            return;

        EstadoAlimento foodState = playerFood.GetComponent<EstadoAlimento>();
        if (foodState != null && IsBlockedFoodState(foodState.estado))
            return;

        FindIngredientIndex(playerFood.name);

        GameObject stationFood = Instantiate(playerFood, StationParent.transform.position, playerFood.transform.rotation, StationParent.transform);
        stationFood.name = playerFood.name;

        Destroy(playerFood);

        OnProcessStarted();
        corrutina = StartCoroutine(ProcessFood(stationFood));
    }

    private IEnumerator ProcessFood(GameObject stationFood)
    {
        ShowSlider();

        float elapsedTime = 0f;

        while (elapsedTime < duracion)
        {
            elapsedTime += Time.deltaTime;
            float progress = duracion <= 0f ? 1f : Mathf.Clamp01(elapsedTime / duracion);

            UpdateSlider(progress);
            PlayProcessAudio(progress);

            if (Input.GetKeyDown(KeyCode.E) && IsPlayerInteractingWithThisCounter && PlayerHandsAreEmpty())
            {
                if (CanTakeFood(progress))
                {
                    HideSlider();
                    GiveProcessedFoodToPlayer(stationFood);
                    corrutina = null;
                    yield break;
                }

                if (progress >= 0.99f)
                {
                    MarkAsBurned();
                    corrutina = null;
                    yield break;
                }
            }

            yield return null;
        }

        MarkAsBurned();
        corrutina = null;
    }

    private void TryTakeBurnedFood()
    {
        if (!quemado)
            return;

        if (!Input.GetKeyDown(KeyCode.E))
            return;

        if (!IsPlayerInteractingWithThisCounter)
            return;

        if (!PlayerHandsAreEmpty())
            return;

        HideSlider();
        HideBurnedImage();
        GiveBurnedFoodToPlayer();
        quemado = false;
    }

    private void FindIngredientIndex(string ingredientName)
    {
        indice = 0;
        objetoEncontrado = false;

        if (ingredientes == null)
            return;

        for (int i = 0; i < ingredientes.Count; i++)
        {
            if (ingredientes[i] != null && ingredientes[i].name == ingredientName)
            {
                indice = i;
                objetoEncontrado = true;
                return;
            }
        }
    }

    private void GiveProcessedFoodToPlayer(GameObject stationFood)
    {
        StopAudioReset();

        if (stationFood == null)
            return;

        TipoIngrediente processedIngredient = GetProcessedIngredient();

        if (processedIngredient != null && processedIngredient.prefabIngrediente != null)
        {
            InstantiateInPlayerHand(processedIngredient.prefabIngrediente);
        }
        else
        {
            InstantiateInPlayerHand(stationFood);
        }

        Destroy(stationFood);
        ResetIngredientSearch();
    }

    private TipoIngrediente GetProcessedIngredient()
    {
        if (!objetoEncontrado)
            return null;

        if (ProcessedIngredients == null)
            return null;

        if (indice < 0 || indice >= ProcessedIngredients.Count)
            return null;

        return ProcessedIngredients[indice];
    }

    private void GiveBurnedFoodToPlayer()
    {
        StopAudioReset();

        GameObject stationFood = GetCurrentStationFood();
        if (stationFood != null)
        {
            Destroy(stationFood);
        }

        if (Quemado != null && Quemado.prefabIngrediente != null)
        {
            InstantiateInPlayerHand(Quemado.prefabIngrediente);
        }

        ResetIngredientSearch();
    }

    private void InstantiateInPlayerHand(GameObject prefab)
    {
        if (prefab == null || PadrePlayer == null)
            return;

        GameObject newFood = Instantiate(prefab, PadrePlayer.transform.position, prefab.transform.rotation, PadrePlayer.transform);
        newFood.name = prefab.name;
    }

    private GameObject GetCurrentStationFood()
    {
        if (StationParent == null || StationParent.transform.childCount <= 0)
            return null;

        return StationParent.transform.GetChild(0).gameObject;
    }

    private void MarkAsBurned()
    {
        quemado = true;
        ShowBurnedImage();
        StopAudioReset();
    }

    private void ShowSlider()
    {
        if (slider == null)
            return;

        slider.gameObject.SetActive(true);
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
    }

    private void HideSlider()
    {
        if (slider == null)
            return;

        slider.gameObject.SetActive(false);
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
    }

    private void UpdateSlider(float progress)
    {
        if (slider == null)
            return;

        slider.value = progress;
    }

    private void ShowBurnedImage()
    {
        if (QuemadoImage != null)
        {
            QuemadoImage.enabled = true;
        }
    }

    private void HideBurnedImage()
    {
        if (QuemadoImage != null)
        {
            QuemadoImage.enabled = false;
        }
    }

    private void PlayProcessAudio(float progress)
    {
        if (progress >= 0.6f && !haSonado)
        {
            if (audio != null)
                audio.Play();

            haSonado = true;
        }

        if (progress >= 0.9f && !haSonado2)
        {
            if (audio != null)
                audio.Stop();

            if (audioQuemado != null)
                audioQuemado.Play();

            haSonado2 = true;
        }
    }

    private bool PlayerHandsAreEmpty()
    {
        return PadrePlayer != null && PadrePlayer.transform.childCount <= 0;
    }

    private bool HasRequiredReferences()
    {
        if (PadrePlayer == null)
        {
            Debug.LogWarning("[" + LogName + "] Falta PadrePlayer en " + gameObject.name);
            return false;
        }

        if (StationParent == null)
        {
            Debug.LogWarning("[" + LogName + "] Falta el padre de la estacion en " + gameObject.name);
            return false;
        }

        return true;
    }

    private void ResetIngredientSearch()
    {
        indice = 0;
        objetoEncontrado = false;
    }

    private void StopAudioReset()
    {
        StopExtraAudio();

        if (audioQuemado != null && audioQuemado.isPlaying)
            audioQuemado.Stop();

        if (audio != null && audio.isPlaying)
            audio.Stop();

        haSonado = false;
        haSonado2 = false;
    }
}