using System.Collections.Generic;
using UnityEngine;

public class FryCounter : CookingCounterBase
{
    [Header("Padres")]
    public GameObject PadreFreidora;

    [Header("Listas")]
    public List<TipoIngrediente> fritos;

    [Header("Audio Freidora")]
    public AudioSource audioFriendo;

    protected override string LogName
    {
        get { return "FryCounter"; }
    }

    protected override GameObject StationParent
    {
        get { return PadreFreidora; }
    }

    protected override List<TipoIngrediente> ProcessedIngredients
    {
        get { return fritos; }
    }

    protected override bool IsPlayerInteractingWithThisCounter
    {
        get
        {
            if (counterInt == null)
                return false;

            if (gameObject.name == "Freir")
                return counterInt.Freir;

            if (gameObject.name == "Freir2")
                return counterInt.Freir2;

            return counterInt.Freir || counterInt.Freir2;
        }
    }

    protected override bool IsBlockedFoodState(int estado)
    {
        return estado == 2 || estado == 6 || estado == 7;
    }

    protected override bool CanTakeFood(float progress)
    {
        return progress <= 0.9f;
    }

    protected override void OnProcessStarted()
    {
        if (audioFriendo != null)
            audioFriendo.Play();
    }

    protected override void StopExtraAudio()
    {
        if (audioFriendo != null && audioFriendo.isPlaying)
            audioFriendo.Stop();
    }

    public void Freir()
    {
        StartCookingProcess();
    }
}