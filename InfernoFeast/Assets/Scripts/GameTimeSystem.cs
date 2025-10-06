using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimeSystem : MonoBehaviour
{
    [Header("Duración de los días y semana")]
    public float secondsPerGameDay = 480f;
    public string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    [Header("UI")]
    public GameObject panelFinalDia;
    public TMP_Text horaText;
    public TMP_Text diaText;

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

        
        if (prev < 6f * 3600f && gameTimeInSeconds >= 6f * 3600f)
        {
            StartNewDayAutomatic();
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

    
    private void StartNewDayAutomatic()
    {
        currentDayIndex = (currentDayIndex + 1) % 7;
      
        gameTimeInSeconds = 6 * 3600;
      
        if (panelFinalDia != null) panelFinalDia.SetActive(false);
       
        lastGameTimeInSeconds = gameTimeInSeconds;
    }

    // llamado por el botón (mantiene compatibilidad con tu UI)
    public void ComenzarNuevoDia()
    {
        StartNewDayAutomatic();
        esperandoNuevoDia = false;
    }

    public void CerrarPanel() { panelFinalDia.SetActive(false); }
}
