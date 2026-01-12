using System.Collections.Generic;
using UnityEngine;

public class ClientTableGroup : MonoBehaviour
{
    [Header("Rondas en las que esta mesa está activa (0 = ronda 1)")]
    public List<int> roundsAvailable = new List<int>();

    [Header("Pedido")]
    public TipoIngrediente requiredDish;

    [Header("Referencias")]
    public Transform snapPoint;
    public GameObject client;
    public GameObject table;

    [Header("Estado")]
    [HideInInspector] public bool served = false;

    // NUEVO: indica si el plato está colocado en la mesa
    public bool platePlaced = false;

    public bool IsActiveInRound(int roundIndex)
    {
        return roundsAvailable.Contains(roundIndex);
    }

    public void ActivateForRound()
    {
        served = false;
        platePlaced = false;

        if (client != null) client.SetActive(true);
        if (table != null) table.SetActive(true);
    }

    public void Deactivate()
    {
        if (client != null) client.SetActive(false);
        if (table != null) table.SetActive(false);
    }

    // Se llama cuando el plato correcto es entregado
    public void OnServed()
    {
        served = true;
        platePlaced = true;
        Deactivate();
    }

    // Se llama cuando el plato es incorrecto
    public void OnMissed()
    {
        served = false;
        platePlaced = false;
        Deactivate();
    }
}
