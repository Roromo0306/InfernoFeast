using UnityEngine;
using TMPro;

public class ManagerFinDia : MonoBehaviour
{
    public static ManagerFinDia Instance { get; private set; }

    [Header("Variables del fin del dia")]
    public int dinero;
    public int reputacion;

    [Header("UI Stats")]
    public TMP_Text din;
    public TMP_Text reput;

    private int lastDinero = int.MinValue;
    private int lastReputacion = int.MinValue;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        RefreshUI(true);
    }

    private void Update()
    {
        RefreshUI(false);
    }

    public void AddDayResults(int dineroTurno, int reputacionTurno)
    {
        dinero += dineroTurno;
        reputacion += reputacionTurno;
        RefreshUI(true);
    }

    public void ResetStats()
    {
        dinero = 0;
        reputacion = 0;
        RefreshUI(true);
    }

    public void RefreshUI(bool force)
    {
        if (!force && dinero == lastDinero && reputacion == lastReputacion)
            return;

        lastDinero = dinero;
        lastReputacion = reputacion;

        if (din != null)
            din.text = dinero.ToString("F0");

        if (reput != null)
            reput.text = reputacion.ToString("F0");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}