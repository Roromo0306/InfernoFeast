using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Cocina/Ingredientes", fileName = "IngredienteNuevo")]
public class TipoIngrediente : ScriptableObject
{
    public Ingredientes TipoIngredientes;
    public Transform prefabIngrediente;
    public string NombreIngrediente;
}
