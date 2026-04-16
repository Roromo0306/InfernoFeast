using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameTimeSystem : MonoBehaviour
{
    [Header("Duración de los días y semana")]
    public float secondsPerGameDay = 480f;
    public string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    [Header("Configuración Fade")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.0f;

    [Header("UI")]
    public GameObject panelFinalDia;
    public TMP_Text horaText;
    public TMP_Text diaText;
    public GameObject Stats;
    public TMP_Text StatDinero;
    public TMP_Text StatReputacion;

    [Header("Manager fin del dia")]
    public GameObject Managerfindia;
    public GameObject Cama;

    private float gameTimeInSeconds = 0f;
    private float lastGameTimeInSeconds = 0f;
    private int currentDayIndex = 0;
    private bool esperandoNuevoDia = false;

    void Awake() => DontDestroyOnLoad(this);

    void Start()
    {
        gameTimeInSeconds = 6 * 3600;
        lastGameTimeInSeconds = gameTimeInSeconds;
        panelFinalDia.SetActive(false);
    }

    void Update()
    {
        if (esperandoNuevoDia) return;

        // guarda prev para detectar cruces de 6:00
        float prev = lastGameTimeInSeconds;

        gameTimeInSeconds += Time.deltaTime * (24f * 60f * 60f / secondsPerGameDay);

        if (gameTimeInSeconds >= 24f * 60f * 60f)
            gameTimeInSeconds -= 24f * 60f * 60f;

        
        if (prev < 2f * 3600f && gameTimeInSeconds >= 2f * 3600f)
        {
            ComenzarNuevoDia();
        }

        if (horaText != null) horaText.text = GetFormattedTime();
        if (diaText != null) diaText.text = GetCurrentGameDay();

        lastGameTimeInSeconds = gameTimeInSeconds;
    }

    string GetFormattedTime()
    {
        int totalSeconds = (int)gameTimeInSeconds;
        int horas = totalSeconds / 3600;
        int minutos = (totalSeconds % 3600) / 60;
        return $"{horas:D2}:{minutos:D2}";
    }

    string GetCurrentGameDay()
    {
        return days[currentDayIndex];
    }

    public void ActivarFinDeDia() { if (panelFinalDia != null) panelFinalDia.SetActive(true); }
    public void Dormir() { ActivarFinDeDia(); }

    public void StartNewDayAutomatic()
    {
        StartCoroutine(SequenceStartNewDay());
    }

    private IEnumerator SequenceStartNewDay()
    {
        Stats.gameObject.SetActive(false);

        // Hacemos el fade hacia transparente (esclarecer)
        yield return StartCoroutine(DoFade(0f));

        Dormir don = Cama.GetComponent<Dormir>();
        currentDayIndex = (currentDayIndex + 1) % 7;
        gameTimeInSeconds = 6 * 3600;
        lastGameTimeInSeconds = gameTimeInSeconds;
        don.nuevoDia = false;
        SceneManager.LoadScene("Restaurant");
        if (panelFinalDia != null) panelFinalDia.SetActive(false);

       
    }

    // llamado por el botón (mantiene compatibilidad con tu UI)
    public void ComenzarNuevoDia()
    {
        StartCoroutine(SequenceComenzarNuevoDia());
    }

    private IEnumerator SequenceComenzarNuevoDia()
    {
        // 1. Primero oscurecemos el fondo
        yield return StartCoroutine(DoFade(1f));

        // 2. Luego mostramos las estadísticas
        ManagerFinDia man = Managerfindia.GetComponent<ManagerFinDia>();
        StatDinero.text = man.dinero.ToString("F2");
        StatReputacion.text = man.reputacion.ToString("F2");

        panelFinalDia.SetActive(false);
        Stats.gameObject.SetActive(true);
        esperandoNuevoDia = false;
    }

    // Función auxiliar para el fade
    private IEnumerator DoFade(float targetAlpha)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0;

        // Si vamos a oscurecer, bloqueamos raycasts para que no se pulse nada por error
        if (targetAlpha > 0) fadeCanvasGroup.blocksRaycasts = true;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;

        // Si terminamos de aclarar, dejamos de bloquear raycasts
        if (targetAlpha <= 0) fadeCanvasGroup.blocksRaycasts = false;
    }

    public void CerrarPanel() { panelFinalDia.SetActive(false); }
}
