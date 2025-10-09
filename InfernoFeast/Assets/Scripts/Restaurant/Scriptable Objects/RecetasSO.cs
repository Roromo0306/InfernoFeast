using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Cocina/Recetas", fileName = "RecetasNuevo")]
public class RecetasSO : ScriptableObject
{
    public TipoIngrediente Ingrediente1;
    public TipoIngrediente Ingrediente2;
    public TipoIngrediente Resultado;
}
