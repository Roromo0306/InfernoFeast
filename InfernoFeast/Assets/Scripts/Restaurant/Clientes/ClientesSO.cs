using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NuevosClientes", menuName = "Cliente")]
public class ClientesSO : ScriptableObject
{
    public string nombre;
    public TipoCliente tipo;
    public float tiempoEnMesa;
    public GameObject prefab;
}
