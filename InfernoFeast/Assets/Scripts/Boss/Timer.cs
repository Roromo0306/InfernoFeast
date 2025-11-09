using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour
{
    [Header("Configuración del Timer")]
    public float duration = 90f;   // duración total en segundos
    public bool countDown = true;  // cuenta atrás si true
    [Header("UI")]
    public TextMeshProUGUI timerText; // asegúrate de asignar esto en el inspector

    private float currentTime;
    private bool isRunning = false;

    // evento que notifica que el timer ha terminado (llegó a 0 por cuenta atrás
    // o alcanzó duration si no es cuenta atrás). Suscribirse con +=.
    public Action OnTimerEnd;

    void Start()
    {
        currentTime = duration;
        UpdateUI();
    }

    void Update()
    {
        if (!isRunning) return;

        if (countDown)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                isRunning = false;
                UpdateUI();
                OnTimerEnd?.Invoke();
                return;
            }
        }
        else
        {
            currentTime += Time.deltaTime;
            if (currentTime >= duration)
            {
                currentTime = duration;
                isRunning = false;
                UpdateUI();
                OnTimerEnd?.Invoke();
                return;
            }
        }

        UpdateUI();
    }

    public void StartTimer()
    {
        currentTime = duration; // reinicia el tiempo
        isRunning = true;
        UpdateUI();
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
