using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerFinDia : MonoBehaviour
{
    public static ManagerFinDia Instance { get; private set; }

    [Header("Variables del fin del dia")]
    public int dinero;
    public int reputacion;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void FinDelDia()
    {

    }
}
