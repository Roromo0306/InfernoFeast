using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComandasManager : MonoBehaviour
{
    public static ComandasManager Instance { get; private set; }
    public List<Sprite> NombresComandasTotales = new List<Sprite>();

    public event System.Action<Sprite> OnComandaAdded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void AddComanda(Sprite s)
    {
        if (s == null) return;
        NombresComandasTotales.Add(s);
        OnComandaAdded?.Invoke(s);
    }
}
