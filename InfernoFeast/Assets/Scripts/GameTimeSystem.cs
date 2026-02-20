using System.Globalization;
using TMPro;
using UnityEngine;

public class GameTimeSystem : MonoBehaviour
{
    public static GameTimeSystem Instance;

    [Header("Duración de los días y semana")]
    public float secondsPerGameDay = 480f;
    public string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    [Header("UI")]
    public TMP_Text horaText;
    public TMP_Text diaText;

    private float gameTimeInSeconds;
    private float lastGameTimeInSeconds;
    private int currentDayIndex;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadTime();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        lastGameTimeInSeconds = gameTimeInSeconds;
    }

    void Update()
    {
        float prev = lastGameTimeInSeconds;
        gameTimeInSeconds += Time.deltaTime * (24f * 3600f / secondsPerGameDay);

        if (gameTimeInSeconds >= 24f * 3600f) gameTimeInSeconds -= 24f * 3600f;

        if (prev < 6f * 3600f && gameTimeInSeconds >= 6f * 3600f)
        {
            StartNewDayAutomatic();
        }

        if (horaText) horaText.text = GetFormattedTime();
        if (diaText) diaText.text = GetCurrentGameDay();

        lastGameTimeInSeconds = gameTimeInSeconds;
    }

    string GetFormattedTime()
    {
        int totalSeconds = (int)gameTimeInSeconds;
        int horas = totalSeconds / 3600;
        int minutos = (totalSeconds % 3600) / 60;
        return $"{horas:D2}:{minutos:D2}";
    }

    string GetCurrentGameDay() => days[currentDayIndex];

    private void StartNewDayAutomatic()
    {
        currentDayIndex = (currentDayIndex + 1) % 7;
        gameTimeInSeconds = 6f * 3600f;

        // Llamamos al método público de Calendar
        Calendar.AdvanceDay();

        SaveTime();
    }

    public void SaveTime()
    {
        PlayerPrefs.SetInt("DayIndex", currentDayIndex);
        PlayerPrefs.SetFloat("TimeInSeconds", gameTimeInSeconds);
        PlayerPrefs.Save();
    }

    public void LoadTime()
    {
        currentDayIndex = PlayerPrefs.GetInt("DayIndex", 0);
        gameTimeInSeconds = PlayerPrefs.GetFloat("TimeInSeconds", 6f * 3600f);
    }

    public void ActivarFinDeDia()
    {
        // Aquí puedes activar un panel de "fin de día" si quieres
        // por ejemplo:
        // panelFinalDia.SetActive(true);
        Debug.Log("Fin del día activado");
    }

    // Mantener compatibilidad con tu Bed.cs
    public void Dormir()
    {
        ActivarFinDeDia();
        StartNewDayAutomatic(); // Opcional: puedes avanzar el día automáticamente al dormir
    }
}