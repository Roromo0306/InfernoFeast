using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EmpezarTurno : MonoBehaviour
{
    [Header("Texto Cuenta Atras")]
    public TextMeshProUGUI Cuenta;

    [Header("Player")]
    public GameObject Player;

    [Header("Lista Comandas")]
    public List<Sprite> NombresComandas;
    public List<int> cantidadCom;
    public int comandasCount = 0;

    [Header("Componentes UI Comandas")]
    public List<Sprite> ListaComandas;

    [Header("Referencia a la UI de abierto o cerrado")]
    public Sprite abierto;
    public Sprite cerrado;
    public Image AbCer;

    [HideInInspector] public bool empezado = false;

    [Header("Variables dinero y reputacion")]
    public int dineroTurno;
    public int reputacionTurno;

    [Header("Sonido")]
    public AudioSource audio;

    [Header("Configuracion Turno")]
    public float duracionTurno = 420f;
    public int comandasIniciales = 3;

    [Header("UI Lista Comandas")]
    public Transform contenedorComandas;
    public GameObject prefabComandaUI;
    public int maxComandas = 4;
    public float separacionY = 90f;
    public float desplazamientoEntrada = 500f;

    private readonly List<ComandaActiva> comandasActivas = new List<ComandaActiva>();

    private Coroutine cuentaAtrasCoroutine;
    private bool finDeDiaAplicado = false;

    private void Awake()
    {
        EnsureLists();
        SetClosedVisual();
        UpdateCuentaText(duracionTurno);
    }

    public void TurnoStart()
    {
        if (empezado)
            return;

        EnsureLists();

        if (cuentaAtrasCoroutine != null)
            StopCoroutine(cuentaAtrasCoroutine);

        PrepararNuevoTurno();

        empezado = true;
        finDeDiaAplicado = false;

        if (AbCer != null && abierto != null)
            AbCer.sprite = abierto;

        if (audio != null && !audio.isPlaying)
            audio.Play();

        cuentaAtrasCoroutine = StartCoroutine(CuentAtras());
    }

    private void PrepararNuevoTurno()
    {
        comandasCount = 0;
        dineroTurno = 0;
        reputacionTurno = 0;

        LimpiarComandasUI();
        ElegirComandas();
        EnsureCantidadComSize();
        ResetCantidadCom();
        UpdateCuentaText(duracionTurno);
    }

    private IEnumerator CuentAtras()
    {
        float tiempo = Mathf.Max(0f, duracionTurno);

        while (tiempo > 0f && empezado)
        {
            UpdateCuentaText(tiempo);
            tiempo -= Time.deltaTime;
            yield return null;
        }

        UpdateCuentaText(0f);
        cuentaAtrasCoroutine = null;

        if (empezado)
            TerminarTurno();
    }

    public void TerminarTurno()
    {
        if (!empezado && finDeDiaAplicado)
            return;

        empezado = false;

        InteractuarCounter interactuarCounter = null;
        if (Player != null)
            interactuarCounter = Player.GetComponent<InteractuarCounter>();

        if (interactuarCounter != null)
            interactuarCounter.turnoEmpezado = false;

        if (cuentaAtrasCoroutine != null)
        {
            StopCoroutine(cuentaAtrasCoroutine);
            cuentaAtrasCoroutine = null;
        }

        if (audio != null && audio.isPlaying)
            audio.Stop();

        SetClosedVisual();

        comandasCount = 0;
        ResetCantidadCom();
        LimpiarComandasUI();

        AplicarFinDeDiaUnaVez();
    }

    private void AplicarFinDeDiaUnaVez()
    {
        if (finDeDiaAplicado)
            return;

        finDeDiaAplicado = true;

        if (ManagerFinDia.Instance != null)
        {
            ManagerFinDia.Instance.AddDayResults(dineroTurno, reputacionTurno);
        }
        else
        {
            Debug.LogWarning("[EmpezarTurno] No existe ManagerFinDia en la escena.");
        }

        dineroTurno = 0;
        reputacionTurno = 0;
    }

    private void ElegirComandas()
    {
        EnsureLists();
        NombresComandas.Clear();

        List<Sprite> comandasDisponibles = GetComandasDisponibles();
        int cantidadInicial = Mathf.Max(0, comandasIniciales);

        for (int i = 0; i < cantidadInicial && comandasDisponibles.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, comandasDisponibles.Count);
            NombresComandas.Add(comandasDisponibles[randomIndex]);
            comandasDisponibles.RemoveAt(randomIndex);
        }
    }

    private List<Sprite> GetComandasDisponibles()
    {
        List<Sprite> resultado = new List<Sprite>();

        if (ComandasManager.Instance != null && ComandasManager.Instance.NombresComandasTotales != null)
        {
            for (int i = 0; i < ComandasManager.Instance.NombresComandasTotales.Count; i++)
            {
                Sprite sprite = ComandasManager.Instance.NombresComandasTotales[i];
                if (sprite != null && !resultado.Contains(sprite))
                    resultado.Add(sprite);
            }
        }

        if (resultado.Count == 0 && ListaComandas != null)
        {
            for (int i = 0; i < ListaComandas.Count; i++)
            {
                Sprite sprite = ListaComandas[i];
                if (sprite != null && !resultado.Contains(sprite))
                    resultado.Add(sprite);
            }
        }

        return resultado;
    }

    public bool CanAcceptNewComanda()
    {
        return empezado && comandasCount < maxComandas;
    }

    public void RegistrarComanda(int pedidoIndex)
    {
        EnsureCantidadComSize();

        if (pedidoIndex < 0 || pedidoIndex >= cantidadCom.Count)
            return;

        cantidadCom[pedidoIndex]++;
        comandasCount = Mathf.Clamp(comandasCount + 1, 0, maxComandas);
    }

    public void QuitarComanda(int pedidoIndex)
    {
        EnsureCantidadComSize();

        if (pedidoIndex < 0 || pedidoIndex >= cantidadCom.Count)
            return;

        cantidadCom[pedidoIndex] = Mathf.Max(0, cantidadCom[pedidoIndex] - 1);
        comandasCount = Mathf.Max(0, comandasCount - 1);

        if (NombresComandas != null && pedidoIndex < NombresComandas.Count && NombresComandas[pedidoIndex] != null)
            EliminarComanda(NombresComandas[pedidoIndex].name);
    }

    public void ComandaUI(string nombre)
    {
        if (string.IsNullOrEmpty(nombre))
            return;

        if (comandasActivas.Count >= maxComandas)
            return;

        if (prefabComandaUI == null)
        {
            Debug.LogWarning("[EmpezarTurno] Falta prefabComandaUI.");
            return;
        }

        if (contenedorComandas == null)
        {
            Debug.LogWarning("[EmpezarTurno] Falta contenedorComandas.");
            return;
        }

        Sprite sprite = BuscarSpriteComanda(nombre);

        if (sprite == null)
        {
            Debug.LogWarning("[EmpezarTurno] No se encontró el sprite de la comanda: " + nombre);
            return;
        }

        GameObject nueva = Instantiate(prefabComandaUI, contenedorComandas);
        nueva.name = "Comanda_" + nombre + "_" + comandasActivas.Count;

        Image img = nueva.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("[EmpezarTurno] El prefabComandaUI no tiene componente Image.");
            Destroy(nueva);
            return;
        }

        img.sprite = sprite;

        RectTransform rt = nueva.GetComponent<RectTransform>();
        if (rt != null)
        {
            float targetY = -comandasActivas.Count * separacionY;
            rt.anchoredPosition = new Vector2(desplazamientoEntrada, targetY);
            StartCoroutine(AnimarEntrada(rt, targetY));
        }

        comandasActivas.Add(new ComandaActiva
        {
            objeto = nueva,
            nombre = nombre
        });
    }

    private Sprite BuscarSpriteComanda(string nombre)
    {
        if (ListaComandas != null)
        {
            for (int i = 0; i < ListaComandas.Count; i++)
            {
                if (ListaComandas[i] != null && ListaComandas[i].name == nombre)
                    return ListaComandas[i];
            }
        }

        if (NombresComandas != null)
        {
            for (int i = 0; i < NombresComandas.Count; i++)
            {
                if (NombresComandas[i] != null && NombresComandas[i].name == nombre)
                    return NombresComandas[i];
            }
        }

        return null;
    }

    private IEnumerator AnimarEntrada(RectTransform rt, float targetY)
    {
        if (rt == null)
            yield break;

        Vector2 start = new Vector2(desplazamientoEntrada, targetY);
        Vector2 end = new Vector2(0f, targetY);

        float t = 0f;
        float duracion = 0.25f;

        while (t < duracion)
        {
            t += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(start, end, t / duracion);
            yield return null;
        }

        rt.anchoredPosition = end;
    }

    private void ReordenarComandas()
    {
        for (int i = 0; i < comandasActivas.Count; i++)
        {
            if (comandasActivas[i] == null || comandasActivas[i].objeto == null)
                continue;

            RectTransform rt = comandasActivas[i].objeto.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = new Vector2(0f, -i * separacionY);
        }
    }

    public void EliminarComanda(string nombre)
    {
        if (string.IsNullOrEmpty(nombre))
            return;

        for (int i = 0; i < comandasActivas.Count; i++)
        {
            if (comandasActivas[i] != null && comandasActivas[i].nombre == nombre)
            {
                if (comandasActivas[i].objeto != null)
                    Destroy(comandasActivas[i].objeto);

                comandasActivas.RemoveAt(i);
                ReordenarComandas();
                return;
            }
        }
    }

    private void LimpiarComandasUI()
    {
        for (int i = comandasActivas.Count - 1; i >= 0; i--)
        {
            if (comandasActivas[i] != null && comandasActivas[i].objeto != null)
                Destroy(comandasActivas[i].objeto);
        }

        comandasActivas.Clear();

        if (contenedorComandas != null)
        {
            for (int i = contenedorComandas.childCount - 1; i >= 0; i--)
            {
                Destroy(contenedorComandas.GetChild(i).gameObject);
            }
        }
    }

    private void EnsureLists()
    {
        if (NombresComandas == null)
            NombresComandas = new List<Sprite>();

        if (cantidadCom == null)
            cantidadCom = new List<int>();

        if (ListaComandas == null)
            ListaComandas = new List<Sprite>();
    }

    private void EnsureCantidadComSize()
    {
        EnsureLists();

        while (cantidadCom.Count < NombresComandas.Count)
            cantidadCom.Add(0);

        while (cantidadCom.Count > NombresComandas.Count)
            cantidadCom.RemoveAt(cantidadCom.Count - 1);
    }

    private void ResetCantidadCom()
    {
        EnsureCantidadComSize();

        for (int i = 0; i < cantidadCom.Count; i++)
            cantidadCom[i] = 0;
    }

    private void SetClosedVisual()
    {
        if (AbCer != null && cerrado != null)
            AbCer.sprite = cerrado;
    }

    private void UpdateCuentaText(float tiempo)
    {
        if (Cuenta == null)
            return;

        tiempo = Mathf.Max(0f, tiempo);

        int minutos = Mathf.FloorToInt(tiempo / 60f);
        int segundos = Mathf.FloorToInt(tiempo % 60f);

        Cuenta.text = minutos.ToString("00") + ":" + segundos.ToString("00");
    }

    [System.Serializable]
    public class ComandaActiva
    {
        public GameObject objeto;
        public string nombre;
    }
}