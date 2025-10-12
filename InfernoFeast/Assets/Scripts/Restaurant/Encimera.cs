using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Encimera : MonoBehaviour
{
    public List<RecetasSO> recetas;
    public GameObject PadreEncimera, objeto1, objeto2 = null;
    public TipoIngrediente mezcla;
    public bool TieneObjeto = false;
    void Start()
    {
        
    }

    void Update()
    {
       // Recetas();
    }

    private void Recetas()
    {

        if(PadreEncimera.transform.childCount > 1)
        {
            objeto1 = PadreEncimera.transform.GetChild(0).gameObject;
            TieneObjeto = true;
        }
        else
        {
            objeto1 = null;
            TieneObjeto = false;
        }

        for(int i = 0; i < recetas.Count; i++)
        {
            if (recetas[i].Ingrediente1.name == objeto1.name && recetas[i].name == objeto2.name)
            {
                Destroy(objeto1);
                Destroy(objeto2);

                Instantiate(recetas[i].Resultado, PadreEncimera.transform.position, PadreEncimera.transform.rotation, PadreEncimera.transform);

                objeto1 = objeto2 = null;
            }
            else
            {
                Destroy(objeto1);
                Destroy(objeto2);

                Instantiate(mezcla, PadreEncimera.transform.position, PadreEncimera.transform.rotation, PadreEncimera.transform);

                objeto1 = objeto2 = null;
            }
        }
    }
}
