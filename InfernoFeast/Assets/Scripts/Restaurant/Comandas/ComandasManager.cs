using System.Collections.Generic;
using UnityEngine;

public class ComandasManager : MonoBehaviour
{
    public static ComandasManager Instance { get; private set; }

    public List<Sprite> NombresComandasTotales = new List<Sprite>();

    public event System.Action<Sprite> OnComandaAdded;

    [Header("Configuracion")]
    public bool permitirDuplicados = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureList();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void AddComanda(Sprite s)
    {
        if (s == null)
            return;

        EnsureList();

        if (!permitirDuplicados && ContainsComanda(s))
            return;

        NombresComandasTotales.Add(s);
        OnComandaAdded?.Invoke(s);
    }

    public bool ContainsComanda(Sprite sprite)
    {
        if (sprite == null || NombresComandasTotales == null)
            return false;

        for (int i = 0; i < NombresComandasTotales.Count; i++)
        {
            Sprite current = NombresComandasTotales[i];

            if (current == null)
                continue;

            if (current == sprite || current.name == sprite.name)
                return true;
        }

        return false;
    }

    public void RemoveComanda(Sprite sprite)
    {
        if (sprite == null || NombresComandasTotales == null)
            return;

        for (int i = NombresComandasTotales.Count - 1; i >= 0; i--)
        {
            Sprite current = NombresComandasTotales[i];

            if (current == null || current == sprite || current.name == sprite.name)
                NombresComandasTotales.RemoveAt(i);
        }
    }

    public List<Sprite> GetComandasDisponibles()
    {
        EnsureList();

        List<Sprite> copia = new List<Sprite>();

        for (int i = 0; i < NombresComandasTotales.Count; i++)
        {
            if (NombresComandasTotales[i] != null)
                copia.Add(NombresComandasTotales[i]);
        }

        return copia;
    }

    private void EnsureList()
    {
        if (NombresComandasTotales == null)
            NombresComandasTotales = new List<Sprite>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}