using UnityEngine;

public class Basura : MonoBehaviour
{
    public GameObject PadreJugador;

    public void Eliminar()
    {
        if (PadreJugador == null)
        {
            Debug.LogWarning("[Basura] Falta PadreJugador en " + gameObject.name);
            return;
        }

        if (PadreJugador.transform.childCount <= 0)
            return;

        GameObject objetoEnMano = PadreJugador.transform.GetChild(0).gameObject;
        if (objetoEnMano != null)
        {
            Destroy(objetoEnMano);
        }
    }
}