using UnityEngine;

public enum TipoCliente
{
    Normal,
    VIP
}

[System.Serializable]
public struct Mesa
{
    public Transform posicion;
    public bool ocupada;
}
