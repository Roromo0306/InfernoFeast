using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FryCounter : MonoBehaviour
{
    [Header("Padres")]
    public GameObject PadrePlayer;
    public GameObject PadreFreidora;

    private int Indice;
    private bool ObjetoEncontrado = false;

    [Header("Listas")]
    public List<TipoIngrediente> fritos;
    public List<TipoIngrediente> ingredientes;

    [Header("UI")]
    public Slider slider;
    public float duracion = 7f;

    private Coroutine corrutina = null;

    public InteractuarCounter counterInt;
    public TipoIngrediente Quemado;
    public Image QuemadoImage;

    private bool quemado = false;

    [Header("Audios")]
    private bool haSonado = false;
    private bool haSonado2 = false;
    public AudioSource audio;
    public AudioSource audioQuemado;
    public AudioSource audioFriendo;

    private void Awake()
    {
        if (slider != null)
        {
            slider.gameObject.SetActive(false);
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
        }

        if (QuemadoImage != null)
        {
            QuemadoImage.enabled = false;
        }
    }

    private void Update()
    {
        if (!quemado)
            return;

        if (!Input.GetKeyDown(KeyCode.E))
            return;

        if (!EstaInteractuandoConEsteCounter())
            return;

        if (PadrePlayer != null && PadrePlayer.transform.childCount > 0)
            return;

        OcultarSlider();

        if (QuemadoImage != null)
            QuemadoImage.enabled = false;

        StopAudioReset();
        InstanciarQuemado();
        quemado = false;
    }

    public void Freir()
    {
        if (!TieneReferenciasMinimas())
            return;

        if (corrutina != null || quemado)
            return;

        if (PadrePlayer.transform.childCount <= 0)
            return;

        GameObject hijoPadre = PadrePlayer.transform.GetChild(0).gameObject;
        if (hijoPadre == null)
            return;

        EstadoAlimento estadoAlimento = hijoPadre.GetComponent<EstadoAlimento>();
        if (estadoAlimento != null && (estadoAlimento.estado == 2 || estadoAlimento.estado == 6 || estadoAlimento.estado == 7))
            return;

        BuscarIngrediente(hijoPadre.name);

        GameObject objetoFreidora = Instantiate(hijoPadre, PadreFreidora.transform.position, hijoPadre.transform.rotation, PadreFreidora.transform);
        objetoFreidora.name = hijoPadre.name;

        Destroy(hijoPadre);

        if (audioFriendo != null)
            audioFriendo.Play();

        corrutina = StartCoroutine(ProcesoFreir(objetoFreidora));
    }

    private IEnumerator ProcesoFreir(GameObject objetoFreidora)
    {
        MostrarSlider();

        float tiempoPasado = 0f;

        while (tiempoPasado < duracion)
        {
            tiempoPasado += Time.deltaTime;
            float progreso = duracion <= 0f ? 1f : Mathf.Clamp01(tiempoPasado / duracion);

            if (slider != null)
                slider.value = progreso;

            ReproducirAudioProceso(progreso);

            if (Input.GetKeyDown(KeyCode.E) && EstaInteractuandoConEsteCounter())
            {
                if (PadrePlayer != null && PadrePlayer.transform.childCount <= 0)
                {
                    if (progreso <= 0.9f)
                    {
                        OcultarSlider();
                        Instanciar(objetoFreidora);
                        corrutina = null;
                        yield break;
                    }

                    if (progreso >= 0.99f)
                    {
                        MarcarQuemado();
                        corrutina = null;
                        yield break;
                    }
                }
            }

            yield return null;
        }

        MarcarQuemado();
        corrutina = null;
    }

    private void BuscarIngrediente(string nombreIngrediente)
    {
        Indice = 0;
        ObjetoEncontrado = false;

        if (ingredientes == null)
            return;

        for (int i = 0; i < ingredientes.Count; i++)
        {
            if (ingredientes[i] != null && ingredientes[i].name == nombreIngrediente)
            {
                Indice = i;
                ObjetoEncontrado = true;
                return;
            }
        }
    }

    private void Instanciar(GameObject objetoFreidora)
    {
        StopAudioReset();

        if (objetoFreidora == null)
            return;

        if (ObjetoEncontrado && fritos != null && Indice >= 0 && Indice < fritos.Count && fritos[Indice] != null && fritos[Indice].prefabIngrediente != null)
        {
            InstanciarEnPlayer(fritos[Indice].prefabIngrediente);
        }
        else
        {
            InstanciarEnPlayer(objetoFreidora);
        }

        Destroy(objetoFreidora);
        ResetEstadoIngrediente();
    }

    private void InstanciarQuemado()
    {
        StopAudioReset();

        GameObject objetoActual = ObtenerObjetoDelCounter();
        if (objetoActual != null)
        {
            Destroy(objetoActual);
        }

        if (Quemado != null && Quemado.prefabIngrediente != null)
        {
            InstanciarEnPlayer(Quemado.prefabIngrediente);
        }

        ResetEstadoIngrediente();
    }

    private void InstanciarEnPlayer(GameObject prefab)
    {
        if (prefab == null || PadrePlayer == null)
            return;

        GameObject nuevoObjeto = Instantiate(prefab, PadrePlayer.transform.position, prefab.transform.rotation, PadrePlayer.transform);
        nuevoObjeto.name = prefab.name;
    }

    private GameObject ObtenerObjetoDelCounter()
    {
        if (PadreFreidora == null || PadreFreidora.transform.childCount <= 0)
            return null;

        return PadreFreidora.transform.GetChild(0).gameObject;
    }

    private void MarcarQuemado()
    {
        quemado = true;

        if (QuemadoImage != null)
            QuemadoImage.enabled = true;

        StopAudioReset();
    }

    private void MostrarSlider()
    {
        if (slider == null)
            return;

        slider.gameObject.SetActive(true);
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
    }

    private void OcultarSlider()
    {
        if (slider == null)
            return;

        slider.gameObject.SetActive(false);
        slider.value = 0f;
    }

    private void ReproducirAudioProceso(float progreso)
    {
        if (progreso >= 0.6f && !haSonado)
        {
            if (audio != null)
                audio.Play();

            haSonado = true;
        }

        if (progreso >= 0.9f && !haSonado2)
        {
            if (audio != null)
                audio.Stop();

            if (audioQuemado != null)
                audioQuemado.Play();

            haSonado2 = true;
        }
    }

    private bool EstaInteractuandoConEsteCounter()
    {
        if (counterInt == null)
            return false;

        if (gameObject.name == "Freir")
            return counterInt.Freir;

        if (gameObject.name == "Freir2")
            return counterInt.Freir2;

        return counterInt.Freir || counterInt.Freir2;
    }

    private bool TieneReferenciasMinimas()
    {
        if (PadrePlayer == null)
        {
            Debug.LogWarning("[FryCounter] Falta PadrePlayer en " + gameObject.name);
            return false;
        }

        if (PadreFreidora == null)
        {
            Debug.LogWarning("[FryCounter] Falta PadreFreidora en " + gameObject.name);
            return false;
        }

        return true;
    }

    private void ResetEstadoIngrediente()
    {
        Indice = 0;
        ObjetoEncontrado = false;
    }

    private void StopAudioReset()
    {
        if (audioFriendo != null && audioFriendo.isPlaying)
            audioFriendo.Stop();

        if (audioQuemado != null && audioQuemado.isPlaying)
            audioQuemado.Stop();

        if (audio != null && audio.isPlaying)
            audio.Stop();

        haSonado = false;
        haSonado2 = false;
    }
}