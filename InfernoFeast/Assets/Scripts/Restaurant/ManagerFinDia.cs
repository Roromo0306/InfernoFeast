using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Update()
    {
        din.text = dinero.ToString("F0");
        reput.text = reputacion.ToString("F0");
    }
}
