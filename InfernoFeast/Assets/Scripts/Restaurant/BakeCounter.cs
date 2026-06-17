using System.Collections.Generic;
using UnityEngine;

public class BakeCounter : CookingCounterBase
{
    [Header("Padres")]
    public GameObject PadreHorno;

    [Header("Listas")]
    public List<TipoIngrediente> horneados;

    protected override string LogName
    {
        get { return "BakeCounter"; }
    }

    protected override GameObject StationParent
    {
        get { return PadreHorno; }
    }

    protected override List<TipoIngrediente> ProcessedIngredients
    {
        get { return horneados; }
    }

    protected override bool IsPlayerInteractingWithThisCounter
    {
        get
        {
            if (counterInt == null)
                return false;

            if (gameObject.name == "Horno")
                return counterInt.Hornear;

            if (gameObject.name == "Horno2")
                return counterInt.Hornear2;

            return counterInt.Hornear || counterInt.Hornear2;
        }
    }

    protected override bool IsBlockedFoodState(int estado)
    {
        return estado == 4 || estado == 6 || estado == 7;
    }

    public void Hornear()
    {
        StartCookingProcess();
    }
}