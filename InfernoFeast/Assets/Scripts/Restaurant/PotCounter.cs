using System.Collections.Generic;
using UnityEngine;

public class PotCounter : CookingCounterBase
{
    [Header("Padres")]
    public GameObject PadrePot;

    [Header("Listas")]
    public List<TipoIngrediente> hervidos;

    protected override string LogName
    {
        get { return "PotCounter"; }
    }

    protected override GameObject StationParent
    {
        get { return PadrePot; }
    }

    protected override List<TipoIngrediente> ProcessedIngredients
    {
        get { return hervidos; }
    }

    protected override bool IsPlayerInteractingWithThisCounter
    {
        get
        {
            if (counterInt == null)
                return false;

            if (gameObject.name == "Hervir")
                return counterInt.Hervir;

            if (gameObject.name == "Hervir2")
                return counterInt.Hervir2;

            return counterInt.Hervir || counterInt.Hervir2;
        }
    }

    protected override bool IsBlockedFoodState(int estado)
    {
        return estado == 3 || estado == 6 || estado == 7;
    }

    public void Hervir()
    {
        StartCookingProcess();
    }
}