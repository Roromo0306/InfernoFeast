using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameTimeSystem : MonoBehaviour
{
    [Header("Configuración de tiempo")]
    public float secondsPerGameDay = 480f; //Esto equivale a 8 minutos en la vida real
    public string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    [Header("UI de transición")]
    public GameObject panelFinalDia;

    [Header("UI de reloj")]
    public TMP_Text horaText;
    public TMP_Text diaText;

    private float gameTimeInSeconds = 0f;
    private int currentDayIndex = 0;
    private bool esperandoNuevoDia = false;

    // Start is called before the first frame update

    public void Awake()
    {
        DontDestroyOnLoad(this);
    }
    void Start()
    {
        gameTimeInSeconds = 6 * 3600; //El primer día comienza a las 6:00 AM
        panelFinalDia.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (esperandoNuevoDia)
        {
            return;
        }

        gameTimeInSeconds += Time.deltaTime * (24f*60*60/secondsPerGameDay);

        //Si pasa de 24h, reinicia el ciclo
        if(gameTimeInSeconds >= 24f * 60f * 60f)
        {
            gameTimeInSeconds -= 24f * 60f * 60f;
        }

        int horas = (int)(gameTimeInSeconds / 3600);
        int minutos = ((int)gameTimeInSeconds % 3600) / 60;

        if(horas ==2 && minutos == 0)
        {
            ActivarFinDeDia();
        }

        //Actualizamos la UI
        if(horaText != null)
        {
            horaText.text = GetFormattedTime();
        }
        if (diaText != null)
        {
            diaText.text = GetCurrentGameDay();
        }
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
        int horas = (int)gameTimeInSeconds/3600;
        if (horas < 2)//A las 2:00 am sigue siendo el dia anterior
        {
            int diaAnterior = (currentDayIndex - 1 + 7) % 7;
            return days[diaAnterior];
        }

        return days[currentDayIndex];
    }

    public void ActivarFinDeDia()
    {
        
        if(panelFinalDia != null)
        {
            panelFinalDia.SetActive(true);
        }
    }
    public void Dormir()//Esto se activa en el codigo de la cama
    {
        ActivarFinDeDia();
    }
    public void ComenzarNuevoDia() //Esto lo lleva el boton del panel de dormir
    {
        currentDayIndex = (currentDayIndex + 1) % 7;
        gameTimeInSeconds = 6 * 3600; //Reinicia a las 6:00 AM
        esperandoNuevoDia = false;

        if(panelFinalDia != null)
        {
            panelFinalDia.SetActive(false);
        }
    }

    public void CerrarPanel()
    {
        panelFinalDia.SetActive(false);
    }
}
