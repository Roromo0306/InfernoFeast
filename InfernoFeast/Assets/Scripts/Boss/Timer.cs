using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [Header("Configuración del Timer")]
    public float duration = 90f;   // duración total en segundos
    public bool countDown = true;  // cuenta atrás si true
    [Header("UI")]
    public TextMeshProUGUI timerText; // asegúrate de asignar esto en el inspector

    private float currentTime;
    private bool isRunning = false;

    void Start()
    {
        // Inicializa el tiempo pero no inicia el timer automáticamente
        currentTime = duration;
        UpdateUI();
    }

    void Update()
    {
        if (!isRunning) return;

        // Actualizamos el tiempo
        if (countDown)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                StopTimer();
            }
        }
        else
        {
            currentTime += Time.deltaTime;
            if (currentTime >= duration)
            {
                currentTime = duration;
                StopTimer();
            }
        }

        UpdateUI();
    }

    public void StartTimer()
    {
        currentTime = duration; // reinicia el tiempo
        isRunning = true;
        UpdateUI(); // asegura que se muestre inmediatamente
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    private void UpdateUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
    }
}
