using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PotCounter : MonoBehaviour
{
    [Header("Padres")]
    public GameObject PadrePlayer; //Objeto padre del player que lleva los objetos
    public GameObject PadrePot; //Objeto padre de la hervidora donde se instancian la comida cuando se deposita

    private int Indice; //Variable de referencia del indice de la lista
    private bool ObjetoEncontrado = false; //Con este bool detectare si se ha encontrado un nombre en el if

    [Header("Listas")]
    public List<TipoIngrediente> hervidos; //Lista de ingredientes ya hervidos
    public List<TipoIngrediente> ingredientes; //Lista de ingredientes compatibles para hervir

    [Header("UI")]
    public Slider slider; //Referencia al slider
    public float duracion = 7f; //Duracion del objeto hasta que termine el slider

    private Coroutine corrutina = null; //Referencia a corrutina

    public InteractuarCounter counterInt; //Referencia la codigo que usa el player para interactuar con los counter
    public TipoIngrediente Quemado; //Referencia al objeto quemado

    public Image QuemadoImage; //Imagen de quemado que sale

    public bool quemado = false; //Bool que indica si ya se ha quemado el objeto

    [Header("Audios")]
    private bool haSonado = false, haSonado2 = false; //Bool para saber si ha sonado el sonido de quemado
    public AudioSource audio; //Referencia al componente audioSource
    public AudioSource audioQuemado;

    public void Hervir()
    {
        GameObject HijoPadre = PadrePlayer.transform.GetChild(0).gameObject; //Guardamos el gameobject que carga el player en un gameobject nuevo

        //Detecto el componente Estado Alimento para saber si dejar que el elemento se pueda freir
        EstadoAlimento Est = HijoPadre.GetComponent<EstadoAlimento>();
        if (Est.estado == 3 || Est.estado == 6 || Est.estado == 7)
        {
            return;
        }

        //Con este for recorre la lista entera hasta que encuentra un objeto que se llama igual que el objeto que lleva el jugador. Al encontrar esto, activo el bool y guardo el indice
        for (int i = 0; i < ingredientes.Count; i++)
        {
            if (ingredientes[i].name == HijoPadre.name)
            {
                Indice = i;
                ObjetoEncontrado = true;
                break;
            }
        }

        GameObject objetoPot = Instantiate(HijoPadre, PadrePot.transform.position, HijoPadre.transform.rotation, PadrePot.transform);
        objetoPot.name = HijoPadre.name;
        Destroy(HijoPadre);

        corrutina = StartCoroutine(ProcesoHervir(objetoPot));
    }

    private void Instanciar(GameObject HijoPadre)
    {
        StopAudioReset();

        //Si el bool es true pasa lo siguiente
        if (ObjetoEncontrado)
        {
            Destroy(HijoPadre); //Destruyo el objeto que llevaba el jugador


            GameObject nuevoObjeto = Instantiate(hervidos[Indice].prefabIngrediente, PadrePlayer.transform.position, hervidos[Indice].prefabIngrediente.transform.rotation, PadrePlayer.transform); //Instancio el objeto equivalente en la lista de hervidos
            nuevoObjeto.name = hervidos[Indice].prefabIngrediente.name; //Me aseguro que el nombre del nuevo objeto instanciado sea el correcto

            Indice = 0;
            ObjetoEncontrado = false;
        }
        else
        {
            GameObject nuevoObjeto = Instantiate(HijoPadre, PadrePlayer.transform.position, HijoPadre.transform.rotation, PadrePlayer.transform); //Instancio el mismo objeto que llevaba el jugador
            nuevoObjeto.name = HijoPadre.name; //Me aseguro que el nombre sea el correcto

            Destroy(HijoPadre); //Destruyo el objeto que llevaba el jugador
        }
    }

    private void InstanciarQuemado()
    {
        StopAudioReset();

        GameObject PadrePot = this.gameObject.transform.GetChild(0).gameObject;
        Destroy(PadrePot.gameObject.transform.GetChild(0).gameObject); //Destruyo el objeto que llevaba el jugador

        GameObject nuevoObjeto = Instantiate(Quemado.prefabIngrediente, PadrePlayer.transform.position, Quemado.prefabIngrediente.transform.rotation, PadrePlayer.transform); //Instancio el objeto equivalente en la lista de horneados
        nuevoObjeto.name = Quemado.prefabIngrediente.name; //Me aseguro que el nombre del nuevo objeto instanciado sea el correcto

        Indice = 0;
    }

    private IEnumerator ProcesoHervir(GameObject objetoPot)
    {
        //Preparamos el slider
        slider.gameObject.SetActive(true);
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;

        yield return null;
        float tiempoPasado = 0f;
        while (tiempoPasado < duracion)
        {
            tiempoPasado += Time.deltaTime;
            slider.value = Mathf.Clamp01(tiempoPasado / duracion); //Fija el valor

            if(this.gameObject.name == "Hervir")
            {
                if (slider.value >= 0.6 && !haSonado)
                {
                    audio.Play();
                    haSonado = true;
                }

                if (slider.value >= 0.9 && !haSonado2)
                {
                    audioQuemado.Play();
                    haSonado2 = true;
                }

                if (Input.GetKeyDown(KeyCode.E) && counterInt.Hervir)
                {
                    if (PadrePlayer.transform.childCount <= 0)
                    {
                        //Se cancela

                        if (slider.value >= 0.6 && slider.value <= 0.9f)
                        {
                            slider.gameObject.SetActive(false);
                            slider.value = 0f;
                            Instanciar(objetoPot);
                            yield break;
                        }

                        if (slider.value >= 0.99f)
                        {
                            quemado = true;
                            QuemadoImage.enabled = true;
                            StopAudioReset();
                            yield break;
                        }
                    }
                }
            }

            if (this.gameObject.name == "Hervir2")
            {

                if (slider.value >= 0.6 && !haSonado)
                {
                    audio.Play();
                    haSonado = true;
                }

                if (slider.value >= 0.9 && !haSonado2)
                {
                    audioQuemado.Play();
                    haSonado2 = true;
                }

                if (Input.GetKeyDown(KeyCode.E) && counterInt.Hervir2)
                {
                    if (PadrePlayer.transform.childCount <= 0)
                    {
                        //Se cancela

                        if (slider.value >= 0.6 && slider.value <= 0.9f)
                        {
                            slider.gameObject.SetActive(false);
                            slider.value = 0f;
                            Instanciar(objetoPot);
                            yield break;
                        }

                        if (slider.value >= 0.99f)
                        {
                            quemado = true;
                            QuemadoImage.enabled = true;
                            StopAudioReset();
                            yield break;
                        }
                    }
                }
            }


            yield return null;
        }

        //Completa el bake
        quemado = true;
        QuemadoImage.enabled = true;
        audio.loop = true;
        audio.Play();

        corrutina = null;
        yield break;
    }

    private void Update()
    {
        if (this.gameObject.name == "Hervir")
        {
            if (quemado && Input.GetKeyDown(KeyCode.E) && counterInt.Hervir)
            {
                slider.gameObject.SetActive(false);
                slider.value = 0f;
                QuemadoImage.enabled = false;
                StopAudioReset();
                InstanciarQuemado();
                quemado = false;
                audio.loop = false;
            }
        }

        if (this.gameObject.name == "Hervir2")
        {
            if (quemado && Input.GetKeyDown(KeyCode.E) && counterInt.Hervir2)
            {
                slider.gameObject.SetActive(false);
                slider.value = 0f;
                QuemadoImage.enabled = false;
                StopAudioReset();
                InstanciarQuemado();
                quemado = false;
                audio.loop = false;
            }
        }

    }

    private void StopAudioReset()
    {
        if (audioQuemado != null && audioQuemado.isPlaying) audioQuemado.Stop();

        if (audio != null && audio.isPlaying && !audio.loop) audio.Stop();

        haSonado = false;
        haSonado2 = false;

        corrutina = null;
    }
}
