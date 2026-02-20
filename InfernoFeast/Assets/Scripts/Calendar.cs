using UnityEngine;

public class Calendar : MonoBehaviour
{
    public delegate void DayChanged();
    public static event DayChanged OnDayChanged;

    // Método público para disparar el evento
    public static void AdvanceDay()
    {
        if (OnDayChanged != null)
            OnDayChanged.Invoke(); // solo se invoca dentro de Calendar
    }
}